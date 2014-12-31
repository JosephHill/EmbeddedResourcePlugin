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
		/// <param name="sourceDirectory">Path to locate embedded resources in assembly.</param>
		/// <param name="recursive">Descend into subdirectories to retrieve resources.  
		/// Recursive writes will treat "." as path delimiters (excluding a final "." for the file extension). 
		/// Non-recursive treats all "." as part of the file name.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the folder is written
		/// </returns>
		public async Task WriteFolder (string sourceDirectory, string targetDirectory = "", bool recursive = true) {
			await WriteFolder (
				this.Assembly, 
				sourceDirectory, 
				targetDirectory, 
				recursive);
		}

		/// <summary>
		/// Writes an embedded resource to the file system 
		/// </summary>
		/// <param name="fileName">Path and name to write the resource to the filesystem.</param>
		/// <param name="resource">Full name of embedded resource in <see cref="Assembly"/>.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the file is written
		/// </returns>
		public async Task WriteResource (string fileName, string resource) {
			await WriteResource (
				this.Assembly,
				fileName, 
				resource);
		}


		/// <summary>
		/// Writes all of the embedded resources in a project folder to the target directory
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="sourceDirectory">Path to locate embedded resources in assembly.</param>
		/// <param name="recursive">Descend into subdirectories to retrieve resources.  
		/// Recursive writes will treat "." as path delimiters (excluding a final "." for the file extension). 
		/// Non-recursive treats all "." as part of the file name.</param>
		/// <returns>
		/// A <see cref="Task"/> which will complete after the folder is written
		/// </returns>
		public static async Task WriteFolder (Assembly assembly, string sourceDirectory, string targetDirectory = "", bool recursive = true) {
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
					resource);
			}
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
		public static async Task WriteResource (Assembly assembly, string fileName, string resource) {
			var rootFolder = FileSystem.Current.LocalStorage;

			//			if (await rootFolder.CheckExistsAsync(fileName) == ExistenceCheckResult.FileExists)
			//				return;

			var folderName = fileName.Substring (
				0, 
				fileName.LastIndexOf (PortablePath.DirectorySeparatorChar));

			var folder = await rootFolder.CreateFolderAsync(
				folderName,
				CreationCollisionOption.OpenIfExists);

			var file = await folder.CreateFileAsync(
				fileName.Substring(fileName.LastIndexOf (PortablePath.DirectorySeparatorChar) + 1),
				CreationCollisionOption.ReplaceExisting);

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

