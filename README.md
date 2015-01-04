LED-Cube-Animation-Tool
=======================

A fast and easy way to make animations for your LED cube.  
Exports to CSV format.  
Use it in the browser at <a href="www.victormoller.com/ledcube">victormoller.com/ledcube</a>


## How to use
1. Click on the lights(balls) to turn them on/off.
2. Create all the frames and animate them.
3. Press Export and then "Copy to clipboard(C Array)"
4. Open up the LEDCubeAnimation.ino file.
5. In there set CUBESIZE to your LEDcube size.
6. Set LEDPin[] to the pins for the LEDcube columns.
7. Set PlanePin[] to the planes on the LEDcube.
8. In PatternTable[] paste in your exported animation in it.
9. Run the Arduino program.


## The CSV format
An exemple how one output line will look. (B is just for C array)  
B100, B000, B000, B000, B000, B000, B000, B000, B000, 5,  
B100 is one row where the lamp status is set from left to right.  
3 (or depending on your cube size) makes up a plane from front to back.  
And then the planes go from bottom to the top.