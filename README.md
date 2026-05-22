

## Project Overview
**[Nicolas Alchemist]** is a Virtual Reality (VR) simulation game designed to provide players with an immersive potion brewing and crafting experience. 

This project was developed strictly for educational, testing, and experimental purposes as part of a university course assignment. The primary objective is to analyze how heavy VR interactions affect hardware performance and to build highly optimized systems within the Unity XR ecosystem. As a non-commercial project, it serves as a strong portfolio piece showcasing my technical competencies in diegetic game design, dynamic material management, zero-allocation dual-grip interaction mechanics, and modular tag-based crafting systems.

## Technologies Used
* **Game Engine:** Unity 6
* **Programming Language:** C#
* **VR Framework:** XR Interaction Toolkit (XRIT)
* **3D Modeling & Optimization:** Blender (Low-poly asset creation and mesh separation)
* **Rendering Pipeline:** Universal Render Pipeline (URP)
* **Version Control:** Git & GitHub

## Key Features
* **Dynamic Cauldron Chemistry:** A tag-based, code-reusable alchemy engine that instantly calculates and updates URP material colors in real-time based on the specific ingredients introduced to the cauldron.
* **Optimized Dual-Grip System:** A high-performance VR grab architecture that dynamically detects the gripping hand (left/right) and utilizes `StringComparison.OrdinalIgnoreCase` to completely eliminate garbage collection (GC) allocation, ensuring a seamless experience free of memory leaks.
* **Global Item Manager:** Centrally controls the game loop using a clean Manager Pattern instead of overloading scene objects with individual scripts. It flawlessly respawns destroyed or lost objects at their exact initial coordinates.
* **Diegetic Feedback:** An immersive interaction design that completely replaces traditional screen-space user interfaces (UI) with physical object destruction, context-specific particle effects, and dynamic color shifts to naturally guide the player.


## About the Developer
I am a Digital Game Design student at Kahramanmaraş İstiklal University. I primarily focus on rapid prototyping, systems programming, VR interaction architectures, and 3D asset optimization (Blender) using Unity 6 and C#.

* **LinkedIn:** https://www.linkedin.com/in/abdullah-%C3%A7elik-35b06b205/


## Academic Notice & License
Copyright (c) 2026 Abdullah Çelik

All Rights Reserved.

This project was created purely for educational, prototyping, and testing purposes within a university curriculum. 

The content of this repository (including source code, 3D models, script architectures, and all other assets) is shared publicly strictly for academic evaluation and portfolio demonstration. Commercial use, distribution, or modification of any materials within this project is strictly prohibited without explicit permission.
