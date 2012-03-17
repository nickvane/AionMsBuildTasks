For those of you who have the '*pleasure*' of integrating CA Aion apps in an automated build, I give you:

# CA Aion MsBuildTasks #

**Aion.MsBuildTasks** are 2 msbuild tasks for building CA Aion apps ([CA Aion Business Rules Expert](http://www.ca.com/us/products/detail/CA-Aion-Business-Rules-Expert.aspx)).

* **Aion.MsBuildTasks.AppBuildOrderTask**: this task will identify all .app files in a give directory and will decide the order in which they will need to be build by looking at the includes of each app. 
* **Aion.MsBuildTasks.BuildAionTask**: this task will build a single app file.

## Setup for integrating Aion in a version control system ##

The application these tasks were made for is using subversion for version control. There are lots of .app files with multiple levels of includes. Because Aion doesn't handle the libincpath paths very well we have decided on using an absolute path in all of the libincpath paths that is pointed to

> C:\ApplicationName

In our buildscript we make a symbolic link (via mklink) from our source folder to the absolute path decided on so that each developer and the buildserver can check out the code to a directory of their choice. For usage see the MapSourceDirectory target in the Build.target file.

## Usage ##

See Build.target for building 3 main .app files. This will identify all the includes necessary to build the .app's, will restore the code from the app and will build only the necessary .app files and in the correct order.

* SourceDir is the directory where all the .app files are in
* StartApplications is a list of all the main .app files
* BuildTimeOutInSeconds: because the Aion build sometimes pops up message boxes when an invalid code file is being build, a timeout has been built in the buildtask that will kill the build process and fail the build.

<pre>
	<Target Name="BuildAion">
		<ItemGroup>
		  <SourceDir Include='$(SourceDirectory)' />
		</ItemGroup>
		<ItemGroup>
		  <StartApplications Include="%(SourceDir.FullPath)\FirstMain.app" />
		  <StartApplications Include="%(SourceDir.FullPath)\SecondMain.app" />
		  <StartApplications Include="%(SourceDir.FullPath)\ThridMain.app" />
		</ItemGroup>
		<PropertyGroup>
		  <StartApplicationsList>@(StartApplications, '|')</StartApplicationsList>
		</PropertyGroup>

		<MSBuild Projects="Aion.proj" targets="Build" Properties="StartApplication=$(StartApplicationsList);ApplicationSourceDirectory=%(SourceDir.FullPath);ShouldRestoreCodeFromApp=true;BuildTimeOutInSeconds=90"
				StopOnFirstFailure="True" />
	</Target>
</pre>

## App header example ##
In the following example this app (ApplicationDataLib) has 3 includes, 2 from own source and 1 from the Aion class libraries (the one that ends with -C):

* ApplicationSysLib
* ApplicationIOLib
* datalib-C

The LibIncPath holds a comma seperated string of directories where the includes can be found.

<pre>
#Library4
ApplicationDataLib

#Revision
0

#LibOwner
nickvane

#LibModStamp
2012/01/12 13:05

#LibIncPath
C:\ApplicationName\SOURCE

#Includes
ApplicationSysLib
ApplicationIOLib
datalib-C

#LibDLL
ApplicationDataLib

#LibExe
ApplicationDataLib

#ErrHandling
MB

... CODE REMOVED ...
</pre>