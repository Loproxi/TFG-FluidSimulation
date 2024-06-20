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
### 3.1 Adding the Fluid Simulation to the Scene
-  How to add the main fluid simulation package/Tool to a Scene:
  1.  Add the `FluidSimulation` prefab from the Assets to the Scene.
-  How to adjust the Prefab with the needed information:
  2.  After clicking on it you will have to set some things in order for it to work.
- Required Shaders
    - The Compute shader `FluidSimulation2Compute` 
    - The Particle Instancing Shader `Custom/Particle`
  
![ComponentsInFluidSimulationPrefab](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/073a2026-04f0-43cf-87a3-fc47312d5174)

### 3.2 Tool Configuration
- This section explains the purpose of each variable and component.
#### 3.2.1 Fluid Initializer
- This script component is responsible for setting the number of particles, the domain bounds of the simulation, the particle scale and generating the positions of each particle within the domain.
    - The variables in detail are
        - `Num Particles`: Specifies the total number of particles in the simulation.
        - `Particle Collision Scale`: Defines the scale that each particle must have to collide properly with other items (Must be the same as the particle Scale in Particle Rendering).
        - `Min Bounds`: Defines the upper-left vertex of the domain quad.
        - `Max Bounds`: Defines the bottom-right vertex of the domain quad.
#### 3.2.2 Fluid Simulation 
- This script component is responsible for adjusting variables to tailor the fluid behavior to your requirements.
    - The variables in detail are
        - `Smoothing Radius`: The radius used for smoothing calculations in the simulation.
        - `Rest Density`: The target density that particles are trying to achieve (higher values cause particles to be closer together).
        - `Fluid Constant`: A multiplier for each density calculation (higher values cause particles to spread).
        - `Near Density Const`: Increases density when particles are on top of each other.
        - `Collision Damping`: Reduces the velocity of particles after they collide with a wall.
        - `Gravity`: The gravitational force affecting the fluid.
        - `Viscosity`: Controls how viscous (slimy) the fluid is.
        - `Compute`: Reference to the compute shader used for the simulation.
#### 3.2.3 Particle Rendering
- This script component is responsible for setting the looks of the particles.
    - The variables in detail are
        - `Mesh`: Specifies the mesh used for the particles in the simulation.
        - `Particle Instancing Shader`: The shader used for particle instancing.
        - `Particle Scale`: Defines the scale of each particle.
        - `Particle Color`: Defines the color of each particle.

## 4. Basic Usage
### 4.1 Creating a Fluid Source
How to add fluid sources to the environment:
- After setting the FluidSimulation prefab to the scene and setting the shaders.
- Press Play in the Unity Editor to start seeing your fluid simulation.
- Adjust the values from the bounds, as well as the ones that shape your fluid to look good. To increase the number of particles you will have to restart de simulation.
### 4.2 Interacting with the Fluid
Explanation of how to interact with the fluid in real-time:
If you happen to like interacting with the fluid you just have to set some of the colliders that are inside the package.
You will find two scripts:

- FluidCircleCollider: Circle collider that will collide with the fluid providing interactability
- FluidQuadCollider: Quad collider that will collide with the fluid providing interactability
  
![ResultOfthePlayerWithColliders](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/7331a9d5-2f4e-4ef5-89a0-fe09cbc1a32c)
