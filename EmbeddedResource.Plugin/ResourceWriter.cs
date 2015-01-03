using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PCLStorage;

namespace EmbeddedResource.Plugin
{
	public class ResourceWriter
	{
		private Assembly Assembly { get; set; }

		/// <summary>
		/// Creates a new <see cref="ResourceWriter"/>
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		public ResourceWriter (Assembly assembly)
		{
			this.Assembly = assembly;
		}

		/// <summary>
		/// Writes all of the embedded resources in a project folder to the target directory
		/// </summary>
		/// <param name="sourceDirectory">Path to embedded resources in assembly.</param>
		/// <param name="targetDirectory">Path to folder where resources will be written.</param>
		/// <param name="recursive">Descend into subdirectories to retrieve resources.  
		/// Recursive writes will treat "." as path delimiters (excluding a final "." for the file extension). 
		/// Non-recursive treats all "." as part of the file name.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the folder is written
		/// </returns>
		public async Task WriteFolder (string sourceDirectory, string targetDirectory = "", bool recursive = true, CreationCollisionOption option =  CreationCollisionOption.ReplaceExisting) {
			await WriteFolder (
				this.Assembly, 
				sourceDirectory, 
				targetDirectory, 
				recursive,
				option);
		}

		/// <summary>
		/// Writes an embedded resource to the file system 
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="source">Relative path to embedded resource in <see cref="Assembly"/>.</param>
		/// <param name="targetDirectory">Path to folder where resources will be written.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the file is written
		/// </returns>
		public async Task WriteFile (string source, string targetDirectory = "", CreationCollisionOption option =  CreationCollisionOption.ReplaceExisting) {
			await WriteFile (
				this.Assembly, 
				source, 
				targetDirectory,
				option);
		}

		/// <summary>
		/// Writes an embedded resource to the file system 
		/// </summary>
		/// <param name="fileName">Path and name to write the resource to the filesystem.</param>
		/// <param name="resource">Full name of embedded resource in <see cref="Assembly"/>.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the file is written
		/// </returns>
		public async Task WriteResource (string fileName, string resource, CreationCollisionOption option =  CreationCollisionOption.ReplaceExisting) {
			await WriteResource (
				this.Assembly,
				fileName, 
				resource,
				option);
		}


		/// <summary>
		/// Writes all of the embedded resources in a project folder to the target directory
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="sourceDirectory">Path to embedded resources in assembly.</param>
		/// <param name="targetDirectory">Path to folder where resources will be written.</param>
		/// <param name="recursive">Descend into subdirectories to retrieve resources.  
		/// Recursive writes will treat "." as path delimiters (excluding a final "." for the file extension). 
		/// Non-recursive treats all "." as part of the file name.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the folder is written
		/// </returns>
		public static async Task WriteFolder (Assembly assembly, string sourceDirectory, string targetDirectory = "", bool recursive = true, CreationCollisionOption option =  CreationCollisionOption.ReplaceExisting) {
			var sourcePrefix = String.Format ("{0}.{1}.",
				assembly.GetName ().Name,
				sourceDirectory.Replace (PortablePath.DirectorySeparatorChar, '.'));

			foreach (var resource in assembly.GetManifestResourceNames()
				.Where(a => a.StartsWith(sourcePrefix))
				.Select(b => b)) {

				// strip assembly name from resource
				var relativePathAndFilename = resource.Substring (sourcePrefix.Length);

				// .NET embedded resources replace path delimiters with "."
				// on recursive write, turn "." into directory separators

				if (recursive) {
					// assume that the last "." is the file extension delimiter
					var lastDot = relativePathAndFilename.LastIndexOf (".");

					// treat any other "." as a path delimiter
					if (lastDot > -1)
						relativePathAndFilename = 
							relativePathAndFilename.Substring (0, lastDot).Replace ('.', PortablePath.DirectorySeparatorChar) +
							"." +
							relativePathAndFilename.Substring (lastDot + 1);
				}

				await WriteResource (
					assembly,
					PortablePath.Combine (targetDirectory, relativePathAndFilename), 
					resource,
					option);
			}
		}

		/// <summary>
		/// Writes an embedded resource to the file system 
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="source">Relative path to embedded resource in <see cref="Assembly"/>.</param>
		/// <param name="targetDirectory">Path to folder where resources will be written.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the file is written
		/// </returns>
		public static async Task WriteFile (Assembly assembly, string source, string targetDirectory = "", CreationCollisionOption option =  CreationCollisionOption.ReplaceExisting) {
			var resource = String.Format ("{0}.{1}",
				assembly.GetName ().Name,
				source.Replace (PortablePath.DirectorySeparatorChar, '.'));

			var fileName = source.Substring (
				source.LastIndexOf (PortablePath.DirectorySeparatorChar) + 1);
				
			await WriteResource (
				assembly,
				PortablePath.Combine(targetDirectory, fileName),
				resource, 
				option);
		}

		/// <summary>
		/// Writes an embedded resource to the file system 
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="fileName">Path and name to write the resource to the filesystem.</param>
		/// <param name="resource">Full name of embedded resource in <see cref="Assembly"/>.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the file is written
		/// </returns>
		public static async Task WriteResource (Assembly assembly, string fileName, string resource, CreationCollisionOption option =  CreationCollisionOption.ReplaceExisting) {
			var rootFolder = FileSystem.Current.LocalStorage;

			var folderName = fileName.Substring (
				0, 
				fileName.LastIndexOf (PortablePath.DirectorySeparatorChar));

			var folder = await rootFolder.CreateFolderAsync(
				folderName,
				CreationCollisionOption.OpenIfExists);

			var file = await folder.CreateFileAsync(
				fileName.Substring(fileName.LastIndexOf (PortablePath.DirectorySeparatorChar) + 1),
				option);

			using (var input = ResourceLoader.GetEmbeddedResourceStream(assembly, resource))
			using (var output = await file.OpenAsync(FileAccess.ReadAndWrite)) {
				byte[] buffer = new byte[1024];
				int length;
				while ((length = input.Read (buffer, 0, 1024)) > 0)
					output.Write (buffer, 0, length);
				output.Flush ();
			}
		}
	}
}

