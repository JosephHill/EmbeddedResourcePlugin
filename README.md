EmbeddedResourcePlugin
======================
Mobile applications often need to bundle files with the app, such as a SQLite database or static HTML or images, that will later be accessed from the file system. 

The EmbeddedResource Plugin for Xamarin and Windows is a Portable Class Library (PCL) that provides a cross-platform API to read files embedded in a .NET assembly and write these files to disk via [PCL Storage](https://pclstorage.codeplex.com/).  

## Using EmbeddedResource Plugin
Files included in .NET projects can be included as Embedded Resources by setting the Build Action for the file to `EmbeddedResource`.  EmbeddedResource Plugin provides two main methods for writing these resources: `ResourceWriter.WriteFile` and `ResourceWriter.WriteFolder`.

The full name for a resource in the root of a project will be comprised of the assembly followed by the name of the file.  An embedded resource located in a subdirectory of a project will include the relative path to the file in the name, using "." as a path delimeter.  

For example:
```
db.sqlite
```
in the root of MyProject.dll becomes
```
MyProject.db.sqlite
```

and
```
images\icon.png
```
in the images subdirectory of MyProject.dll becomes
```
MyProject.images.icon.png
```

The `WriteFile` and `WriteFolder` methods simplify decoding this naming scheme by allowing you to provide paths to a resource in a project, along with an output path, so that writing an embedded resource feels more like performing a copy operation.  E.g. `writer.WriteFile("www\index.html", "webroot");`

A simple C# class that uses EmbeddedResource Plugin for Xamarin and Windows to initialize data for an app might look like:
```csharp
using PCLStorage;
using Plugin.EmbeddedResource;

namespace Sample
{
	public class SampleLib
	{
		public SampleLib ()
		{
		}

		public static async Task Init(Assembly assembly) {
      var rootFolder = FileSystem.Current.LocalStorage;
			
			var writer = new ResourceWriter (assembly);
      
      // Only need to write the bundled files once.
			if(await rootFolder.CheckExistsAsync("db.sqlite") == ExistenceCheckResult.NotFound) {
				await writer.WriteFile ("App_data/db.sqlite", rootFolder.Path);
				await writer.WriteFolder ("images", rootFolder.Path);
			}
		}
	}
}
```

Notes on usage
==============
When a file is embedded as a resource, it becomes impossible to differentiate a path delimiter "." from a file extension ".".  For example, www/images/big.button.png looks just like www/images/big/button.png.  For this reason, `WriteFolder` always treats the final "." in a resource name as the file extension delimiter by default.  Calling `WriteFolder` with `recursive = false` will do a non-recursive copy of all resources in the specified source path (i.e., all instances of "." will be treated as part of the filename for every file in the specified folder).

Because some platforms^H supported by PCL Storage provide only asynchronous file access, the `ResourceWriter.WriteFile` and `ResourceWriter.WriteFolder` APIs are also asynchronous.  Anything you want to do in your app that depends on these resources having been written to disk (such as showing the data from the database) will need to await the completion of these calls.
