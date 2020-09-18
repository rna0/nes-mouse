## nes gamepad to controlling a Windows Mouse
This Program was made to be a prof of concept but I decided to take it a step further and add some features.

## the NES controller
The game controller used for both the NES and the Famicom features an oblong brick-like design with a simple four button layout: two round buttons labeled "A" and "B", a "START" button, and a "SELECT" button. Additionally, the controllers utilize the cross-shaped joypad, designed by Nintendo employee Gunpei Yokoi for Nintendo Game & Watch systems, to replace the bulkier joysticks on earlier gaming consoles' controllers.

![NESGamepad](https://upload.wikimedia.org/wikipedia/commons/thumb/b/b5/Nintendo-Entertainment-System-NES-Controller-FL.jpg/1024px-Nintendo-Entertainment-System-NES-Controller-FL.jpg)

-----
### Download

You can get the latest build of nes.mouse in the release page since Feb 18.  

![>](https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Ffc06.deviantart.net%2Ffs70%2Ff%2F2012%2F171%2F1%2Ff%2Fnes_controller_icon_by_nickhrh-d546smr.png) [Release nes.mouse.zip](https://github.com/rna0/nes-mouse/releases/tag/nes_mouse) - x86 for Windows 7, 8, and 10 

-----
### Setup

* Buy a Direct input USB NES Controller from the web, I cannot give you links but there are alot.
* A working computer with windows installed
* This program

-----
#### How to open

1. open the "nes.mouse.zip" file and extract "nes mouse" folder to anyware you would like on your computer.

2. connect your USB NES controller to the Computer before the next step.

3. open "nes mouse.exe", and enjoy!


-----
#### Usage

| NES BUTTON        | CURSUR MANIPULATION             |
| ------------- |-------------|
	|A		|Right Click		|
	|B		|Left Click		|
	|START		|Faster DPI		|
	|SELECT		|Slower DPI		|
	|D-Pad DOWN/UP	|Cursur DOWN/UP		|
	|D-Pad LEFT/RIGHT   |Cursur LEFT/RIGHT	|
	
* You may change the selected controller input if you have more than one the program from the program icon on the bottom right corner
* You may exit the program from the program icon on the bottom right corner

-----
#### Tecnologies used

*	the Program tracks Direct Input signals from the controller using the [SlimDX Library](https://github.com/SlimDX/slimdx)
	Buttons are tracked as:
	

| NES BUTTON        | WINDOWS DIRECT INPUT FOR CONTROLLER            |
| ------------- |-------------|
 A                | button 1          
 B                | button 0          
 START            | button 9          
 SELECT           | button 8          
 D-Pad DOWN/UP    | Position Y -/+100 
 D-Pad LEFT/RIGHT | Position X -/+100 

*	user32.dll is used for cursur manipulation


-----
NES controllers are (c) Nintendo at nintendo.com

This application is under the MIT Licence
