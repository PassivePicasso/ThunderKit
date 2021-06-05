# Using Local Thunderstore Sources

Sometimes you need packages not provided by an existing Packages source and will need to create local packages  to support your workflow
This Guide will break down the steps required to do so

Start by creating a LocalThunderstoreSource

![Create LocalThunderstoreSource](https://i.imgur.com/OFXnXQF.png)

Designate a folder where you will store your Thunderstore compatible Zip files

![](https://i.imgur.com/3VIsPOQ.png)

For this example, I'll acquire a copy of BepInEx from the BepInEx github repo, this file isn't configured for usage with Thunderstore so we will need to add the files necessary

![](https://i.imgur.com/EnYQkwn.png)

As you can see, the root of the zip does not contain a manifest.json, icon.png or readme.md
These files are required by Thunderstore and thus also required for ThunderKit's LocalThunderstoreSource

![](https://i.imgur.com/cOku1T9.png)

Create the required files for Thunderstore and populate them accordingly

![](https://i.imgur.com/nE6zfEg.png)

Then copy these files into the zip file

![](https://i.imgur.com/HDLukg9.png)

Now that they are added we have done all the setup.

NOTE: This is not a correct Configuration for BepInEx to use the ThunderKit BepInEx template. Work is being done to make the BepInEx Template more robust.
However this technique will work for most other content, BepInEx is a mod loader and the Pipeline has to make special considerations for setting it up.

Finally lets remove the files left behind

![](https://i.imgur.com/l6KSRmi.png)

Go back into unity, and refresh your LocalThunderstoreSources

![](https://i.imgur.com/6DMoqqi.png)

If it fails to refresh, verify the Path is correctly entered.
After successfully refreshing you should see the package show up here

![](https://i.imgur.com/vcbDCH5.png)

Open Packages and you should now see the Local Thunderstore listed

![](https://i.imgur.com/zgK6Yl2.png)

if you do not see it, click on Refresh
now you can install packages from this local thunderstore source
You can also download and place any packages from Thunderstore into this folder to keep a local cache of commonly used packages