# WinUI FPS Test

## Overview
This project is a minimal reproduction for a bug where a WinUI 3 application's framerate varies based on the operating system and graphics hardware configuration.

## Issue Description
The issue is that the exact same output binary (even with the latest Windows App SDK) exhibits different framerate scaling behavior depending on the OS:
- **Windows 11:** The application runs and scales its FPS correctly according to the monitor's display rate. Tested at 50,60,75 on Intel Integrated Graphics.
- **Windows 10:** The application's FPS is clamped to around ~30 FPS when using Intel Integrated Graphics.

The same PC hardware gives different results when installed with Windows 11 versus Windows 10. 

There are instances where some have achieved 60 FPS using a dedicated graphics card as reported by some testers, but none using Intel Integrated Graphics.

References 

https://github.com/microsoft/microsoft-ui-xaml/issues/10092 - Microsoft closed without proper solution

https://github.com/microsoft/microsoft-ui-xaml/issues/11048 - Still Open, but user has not mentioned a difference in behaviour with Windows 10 and Windows 11.

https://github.com/microsoft/microsoft-ui-xaml/issues/7290 - Similar Issue but reported in the context of Color Picker control, but fixed (may be it was there for Windows 11 and it was fixed for Windows 11)

https://github.com/microsoft/microsoft-ui-xaml/issues/9840 - Similar Issue. But user hasn't reported a difference in behaviour with Windows 10 and Windows 11.
