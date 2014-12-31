using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EmbeddedResource.Plugin
{

	/// <summary>
	/// Utility class that can be used to find and load embedded resources into memory.
	/// </summary>
	public static class ResourceLoader 
	{
		/// <summary>
		/// Attempts to find and return the given resource from within the specified assembly.
		/// </summary>
		/// <returns>The embedded resource stream.</returns>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="resource">Full name of embedded resource in <see cref="Assembly"/>.</param>
		public static Stream GetEmbeddedResourceStream(Assembly assembly, string resource)
		{
			var resourceNames = assembly.GetManifestResourceNames();

			var resourcePaths = resourceNames
				.Where(x => x.EndsWith(resource, StringComparison.CurrentCultureIgnoreCase))
				.ToArray();

			if (!resourcePaths.Any())
			{
				throw new Exception(string.Format("Resource ending with {0} not found.", resource));
			}

			if (resourcePaths.Count() > 1)
			{
				throw new Exception(string.Format("Multiple resources ending with {0} found: {1}{2}", resource, Environment.NewLine, string.Join(Environment.NewLine, resourcePaths)));
			}

			return assembly.GetManifestResourceStream(resourcePaths.Single());
		}

		/// <summary>
		/// Attempts to find and return the given resource from within the specified assembly.
		/// </summary>
		/// <returns>The embedded resource as a byte array.</returns>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="resource">Full name of embedded resource in <see cref="Assembly"/>.</param>
		public static byte[] GetEmbeddedResourceBytes(Assembly assembly, string resource)
		{
			var stream = GetEmbeddedResourceStream(assembly, resource);

			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Attempts to find and return the given resource from within the specified assembly.
		/// </summary>
		/// <returns>The embedded resource as a string.</returns>
		/// <param name="assembly"><see cref="Assembly"/> containing embedded resources.</param>
		/// <param name="resource">Full name of embedded resource in <see cref="Assembly"/>.</param>
		public static string GetEmbeddedResourceString(Assembly assembly, string resource)
		{
			var stream = GetEmbeddedResourceStream(assembly, resource);

			using (var streamReader = new StreamReader(stream))
			{
				return streamReader.ReadToEnd();
			}
		}
	}
}

