
3rd Eye Scene
=============
3rd Eye Scene is a visual debugger and debugging aid in the vain of [rviz](http://wiki.ros.org/rviz) or physics engine viewers such as [Havok Visual Debugger](https://www.havok.com/physics/) or [PhysX Visual Debugger](https://developer.nvidia.com/physx-visual-debugger). Whereas those tools are tightly bound to their respective SDKs, 3rd Eye Scene can be used to remotely visualise and debug any real time or non real time 3D algorithm. Conceptually, it can be thought of as a simple remote rendering application. A 3es server may be embedded into any program, then 3es render commands may be interspersed throughout the program. The 3es viewer client application is then used to view, record and playback these render commands.

Features
--------
- Remote 3D rendering from any application
- Record rendered data
- Playback and step through recorded data
- Open, extensible protocol
- Plugin extensible to visualise specialised geometry

Use Cases
---------
- Visualising geometric algorithms
  - Mesh operations
  - Geometric intersection tests
  - Point cloud processing
- Remote visualisation
  - Visualise 3D data from headless processes 
- Real time visualisation
  - Remote visualisation
  - Visualise "hidden" data
    - Physics geometry
    - AI logic and constructs
- QA testing
  - Record test sessions and attach 3es files to bug reports.

Additional documentation can be found at [https://data61.github.io/3rdEyeScene/](https://data61.github.io/3rdEyeScene/)
