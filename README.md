# HDR

HDR - Recovering High Dynamic Range Radiance Maps from Photographs C# .NET Framework - 2018

Requirements
-----
  - Target framework: .NET Framework 4.6.1
  - C# WPF App
  - Visual Studio 2017
  - NuGet: InteractiveDataDisplay.WPF (Graphs)
  - NuGet: CenterSpace.NMath (Least Square Solution)

Program supports features
-----
  - Create HDR image from multiply images with different exposure times. Note: not displayed as pure HDR, values rounded
  - Apply CLAHE on HDR image
   
How to use
-----
  - Load images with different exposure times
  - Apply CLAHE after HDR image generated
  
Examples
-----
<img src="https://github.com/kruherson1337/HDR/blob/master/example.jpg?raw=true" alt="Example"/>
With CLAHE applied - Window size = 220, Contrast Limit = 5
<img src="https://github.com/kruherson1337/HDR/blob/master/exampleCLAHE.jpg?raw=true" alt="Example"/>

References
-----
  - Article http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.463.6496&rep=rep1&type=pdf
