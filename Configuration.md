
## GitState Configuration
There is currently only basic configuration available.

All settings are stored in file **GitState.cfg**, which is a standard INI-file format.

```
[Settings]
FontSize=11
UpdateIntervalSec=120
RepositoryFolders=C:\ICT Baden;/home/frank/ICT Baden
```


**FontSize**    
The font size to use, in pixel.

**UpdateIntervalSec**    
Interval the repositories state is updated, in seconds.    
⚠ If you have many repositories, this may impact performance.

**RepositoryFolders**
List of folders to be scanned for repository folders, semicolon separated.
