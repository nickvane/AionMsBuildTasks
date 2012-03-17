For those of you who have the '*pleasure*' of integrating CA Aion apps in an automated build, I give you:

# CA Aion MsBuildTasks #

**Aion.MsBuildTasks** are 2 msbuild tasks for building CA Aion apps ([CA Aion Business Rules Expert](http://www.ca.com/us/products/detail/CA-Aion-Business-Rules-Expert.aspx)).

* **Aion.MsBuildTasks.AppBuildOrderTask**: this task will identify all .app files in a give directory and will decide the order in which they will need to be build by looking at the includes of each app. 
* **Aion.MsBuildTasks.BuildAionTask**: this task will build a single app file.

## Setup for integrating Aion in a version control system ##

The application these tasks were made for is using subversion for version control. There are lots of .app files with multiple levels of includes. Because Aion doesn't handle the libincpath paths very well we have decided on using an absolute path in all of the libincpath paths that is pointed to
> C:\ApplicationName

In our buildscript we make a symbolic link (via mklink) from our source folder to the absolute path decided on so that each developer and the buildserver can check out the code to a directory of their choice.

## Usage ##

