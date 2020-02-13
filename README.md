# dalamud-translator-plugin
Translator plugin for FFXIV

Translates chat text from one language to another, placing the translation immediately into your chatbox.
![Translation Exmaple](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_12.png)

# Requirements
* XIVLauncher: https://github.com/goaaats/FFXIVQuickLauncher
* Latest release of plugin: https://github.com/Haplo064/dalamud-translator-plugin/releases

# Installation of plugin
* Extract all files of zip at: ```%AppData%\Roaming\XIVLauncher\```

![Folder Example1](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_01.png)

* Currently (until a further release is made for XIVLauncher, coming "soon") requires adding a property to the launcher to enter preview mode.
To do this, create a shortcut of XIVLauncher, and add the `--dalamudStg` paramater:
![Paramater Exmaple](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_09.png)

# Translation Engine
* Currently Google and Yandex are supported. Google does not require an API Key, but may softban your IP if you process too many requests.
Yandex requires a free API Key, and can handle 1 million characters per month.

# In-game Config
* ```/trn <h/help>``` List of commands.
* ```/trn <e/engine> <1/google or 2/yandex``` To change translation engine Google.
* ```/trn <i/inject> <1/on/true or 0/off/false>``` To change if you want to inject into the ffxiv chat.
* ```/trn <w/window>``` Opens an overlay chat window with only translations.


# Yandex API Key
To enable Yandex, follow these steps:
* Go to: https://translate.yandex.com/developers/keys
* Create an account if you do not have one:
![Create Account](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_04.png)
* Request an API Key:
![Request API Key](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_05.png)
* Copy the key:
![Request API Key](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_07.png)
* To the ```%AppData%\Roaming\XIVLauncher\plugins\translator\config.json``` file.
![Config File](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_02.png)
* Replacing the "XXX" with the API Key.
![Config XXX](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_03.png)
![Config API](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_08.png)

# Notes
* The dev mode of XIVLauncher adds a bar. You can disable it via the "Draw Dalamud dev menu" option.
![Dalamud Dev](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_11.png)
* The plugin will show that it has loaded successfully via a window on game boot.
![Dalamud Dev](https://github.com/Haplo064/dalamud-translator-plugin/blob/master/img/rm_10.png)
