# WinUI FPS Test

## Overview
This project is a minimal reproduction for a bug where a WinUI 3 + Win2d application's framerate varies based on the operating system and graphics hardware configuration.

## Experimental Findings

Tested three different animation loop architectures on the exact same hardware (Intel Integrated Graphics) dual-booting Windows 10 and Windows 11.

### 1. `CompositionTarget.Rendering` + Win2D `CanvasControl`
In this setup, I called `CanvasControl.Invalidate()` inside the `CompositionTarget.Rendering` event. 

* **Windows 11 (60Hz Monitor):**
  * `CompositionTarget.Rendering` fires at **~60 FPS**. However, because `Invalidate()` simply queues a draw request while the compositor is already busy, the actual `CanvasControl.Draw` event is delayed, resulting in an output of **~30 FPS**.
* **Windows 10 (60Hz Monitor):**
  * Windows 10 aggressively throttles `CompositionTarget.Rendering` down to **~30 FPS** (1/2 of 60Hz). Even if I call `Invalidate()` each time, `CanvasControl.Draw` fires at **~16 FPS** (exactly 1/4th of 60Hz).

### 2. `DispatcherQueueTimer` (15ms) + `CanvasControl`
In this setup, I called `CanvasControl.Invalidate()` inside a `DispatcherQueueTimer` ticking every 15ms.

* **Windows 11:** 
  * Reaches **55 to 60 FPS**, but suffers from heavy frame drops and micro-stutters.
* **Windows 10:** 
  * *(Yet to be fully tested).*

### 3. `CanvasAnimatedControl`
In this setup, I used `CanvasAnimatedControl`, setting `IsFixedTimeStep="False"` to allow the control to sync natively with the monitor.

* **Windows 10 & Windows 11:**
  * `Update` and `Draw` events run on a background thread and fire in perfect synchronization with the monitor's actual refresh rate.
  * Successfully maintains **60 FPS** on a 60Hz monitor, and scales to **70 FPS / 75 FPS** when the monitor refresh rate is increased with zero frame drops.

---

## Conclusion
The discrepancies reported in various GitHub issues also have an additional layer of OS differences. When using Win2D, it is always better to use `CanvasAnimatedControl` when doing continuous animations.

## References & Related Issues

* [microsoft-ui-xaml #10092](https://github.com/microsoft/microsoft-ui-xaml/issues/10092) - Closed without a proper solution.
* [microsoft-ui-xaml #11048](https://github.com/microsoft/microsoft-ui-xaml/issues/11048) - Open, but lacks context on the Windows 10 vs Windows 11 OS differences.
* [microsoft-ui-xaml #7290](https://github.com/microsoft/microsoft-ui-xaml/issues/7290) - Similar VSync issue reported in the context of the Color Picker control (believed to be fixed for Windows 11).
* [microsoft-ui-xaml #9840](https://github.com/microsoft/microsoft-ui-xaml/issues/9840) - Similar issue, but does not identify the OS discrepancy.
