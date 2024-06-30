# Documentation for the 2D Fluid Simulation Tool in Unity


## VideoTeaser:
![VideoTeaser](https://youtu.be/LHYGvYJ7tsg)

## 1. Introduction

### 1.1 Description
-  This tool enables the simulation of fluids in a 2D environment within Unity, providing realistic visual effects of moving fluids. This tool uses an already existing fluid simulation method called smoothed particles hydrodynamics. 
-  Example:
![ExampleScene2](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/ffaa8645-a2c3-4c80-98f9-a6cb7866dcc7)

*The assets used in the example scene and the gif above are from [brullov](https://brullov.itch.io/oak-woods)*

### 1.2 Requirements
-  The tool has been tested on Unity 2022.3.3 and 2021.3.3 but it should work on Unity Versions 2020 as well.
-  Since this tool uses compute shaders, it is important to check that your platform support compute shaders in order to this tool to work. Here you have a list of platform that support them (List extracted from Unity Documentation).
    - Windows and Windows Store, with a DirectX 11 or DirectX 12 graphics API and Shader Model 5.0 GPU
    - macOS and iOS using Metal graphics API
    - Android, Linux and Windows platforms with Vulkan API
    - Modern OpenGL platforms (OpenGL 4.3 on Linux or Windows; OpenGL ES 3.1 on Android). Note that Mac OS X does not
      support OpenGL 4.3
    - Modern consoles

## 2. Installation
### 2.1 Download
-  [Link to download the tool package.](https://github.com/Loproxi/TFG-FluidSimulation/releases/tag/v1.0)
### 2.2 Import
-  Step-by-step instructions for importing the package into Unity:
    1. Open Unity and create a new project or open an existing project.
    2. Go to Assets > Import Package > Custom Package.
    3. Select the downloaded package and click Import.
## 3. Initial Setup
### 3.1 Adding the Fluid Simulation to the Scene
1.  How to add the main fluid simulation package/Tool to a Scene:
    -  Add the `FluidSimulation` prefab from the Assets to the Scene.
2.  How to adjust the Prefab with the needed information, In case it was not already set:
    - After clicking on it you will have to set some things in order for it to work.
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
        - `Particle Collision Scale`: Defines the scale that each particle must have to collide properly with other items. This must be the same as the `Particle Scale` in `Particle Rendering`.
        - `Min Bounds`: Defines the upper-left vertex of the domain quad.
        - `Max Bounds`: Defines the bottom-right vertex of the domain quad.
#### 3.2.2 Fluid Simulation 
- This script component is responsible for adjusting variables to tailor the fluid behavior to your requirements.
    - The variables in detail are
        - `Smoothing Radius`: This radius defines the "zone of influence" around each particles in the simulation. The larger the smoothing radius area, the more particles will be taken into account for the calculations. 
        - `Rest Density`: The target density that particles are trying to achieve. Higher values cause particles to be closer together.
        - `Fluid Constant`: A multiplier for each density calculation. Higher values cause particles to spread out more.
        - `Near Density Const`: Avoid particles to be on top of each other.
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

#### Customizable Fluid
![FullyCustomizableFluid5](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/fe6b6a42-897f-40b6-b9e7-cffa06c50f19)

## 4. Basic Usage
### 4.1 Creating a Fluid Source
How to add fluid sources to the environment:
- After setting the FluidSimulation prefab to the scene and setting the shaders.
- Press Play in the Unity Editor to start seeing your fluid simulation.
- Adjust the values from the bounds, as well as the ones that shape your fluid to look good. To increase the number of particles you will have to restart de simulation.
### 4.2 Interacting with the Fluid
Explanation of how to make objects interact with the fluid in real-time:

1. **Create the GameObject:** Start by creating the GameObject that you want the fluid to interact with.
2. **Add the Fluid Collider Scripts:** Add the fluid collider scripts provided in the package:
    - You will find two scripts:
        - FluidCircleCollider: A circular collider that will interact with the fluid.
        - FluidQuadCollider: A rectangular collider that will interact with the fluid.
    - For more complex shapes, you can add multiple colliders to different GameObjects. Check the example below:
      
![PlayerInExample](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/b122a8c8-c3eb-4258-9e0d-6ab8ab6f824f)

**Each square on the photo is a different `FluidQuadCollider` on the same gameobject**

3. Result
![ResultOfthePlayerWithColliders](https://github.com/Loproxi/TFG-FluidSimulation/assets/79161178/7331a9d5-2f4e-4ef5-89a0-fe09cbc1a32c)
