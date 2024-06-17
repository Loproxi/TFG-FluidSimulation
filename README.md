# Documentation for the 2D Fluid Simulation Tool in Unity

## 1. Introduction

### 1.1 Description
-  This tool enables the simulation of fluids in a 2D environment within Unity, providing realistic visual effects of moving fluids. This tool uses an already existing fluid simulation method called smoothed particles hydrodynamics. 
-  Example:
![ExampleScene](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/3f5876be-18f1-42f8-88e9-d6997ce4b115)
![ExampleScene2](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/ffaa8645-a2c3-4c80-98f9-a6cb7866dcc7)

### 1.2 Requirements
-  The tool has been tested on Unity 2022.3.3 but it should work on Unity Versions 2020-2021
-  Since this tool uses compute shaders, it is important to check that your platform support compute shaders in order to this tool to work. Here you have a list of platform that support them (List extracted from Unity Documentation).
    - Windows and Windows Store, with a DirectX 11 or DirectX 12 graphics API and Shader Model 5.0 GPU
    - macOS and iOS using Metal graphics API
    - Android, Linux and Windows platforms with Vulkan API
    - Modern OpenGL platforms (OpenGL 4.3 on Linux or Windows; OpenGL ES 3.1 on Android). Note that Mac OS X does not
      support OpenGL 4.3
    - Modern consoles

## 2. Installation
### 2.1 Download
-  Link to download the tool package.
### 2.2 Import
-  Step-by-step instructions for importing the package into Unity:
    1. Open Unity and create a new project or open an existing project.
    2. Go to Assets > Import Package > Custom Package.
    3. Select the downloaded package and click Import.
## 3. Initial Setup
### 3.1 Adding the Fluid to the Scene
-  How to add the main fluid simulation component to a scene:
  1.  Create a new empty GameObject or select an existing one.
Add the FluidSimulation2D script to the GameObject (Add Component > FluidSimulation2D).
### 3.2 Component Configuration
Explanation of the main properties of the component:
Resolution: Defines the resolution of the simulation.
Viscosity: Controls the fluid's viscosity.
Diffusion Rate: Controls the fluid's diffusion rate.
Gravity: Adjusts the gravity affecting the fluid.
## 4. Basic Usage
### 4.1 Creating a Fluid Source
How to add fluid sources to the environment:
Create a new empty GameObject or select an existing one.
Add the FluidSource2D script to the GameObject (Add Component > FluidSource2D).
Configure the properties of the FluidSource2D component (position, amount of fluid, etc.).
### 4.2 Interacting with the Fluid
Explanation of how to interact with the fluid in real-time:
Use the mouse to add forces to the fluid.
Modify parameters in real-time from the Inspector to see immediate effects.
