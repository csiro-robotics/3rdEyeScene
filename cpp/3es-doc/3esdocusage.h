
/*!
@page docusage Using 3rd Eye Scene Server Code
This page outlines how to use 3es server code to good effect either in debugging real time applications or opaque geometric algorithms.

The most important thing to remember about 3es is that it allows you to visualise data which would otherwise be opaque. 3es supports the software development tenant "make it visible". It is very difficult to debug something you cannot see. 3es server code essentially allows you insert render commands at arbitrary points in your code to remotely visualise, @em record and @em playback the result. Visualisation itself is not enough as things can happen too fast to see. 3es really shines when it is used to record and step through stepping through the playback to visualise what are otherwise single frame events.

@section secusecases Example Use Cases
@subsection subusecase0 Geometry Processing Visualisation
3<sup>rd</sup> Eye Scene can be used to debug and visualise geometry processing algorithms. The C++ example "3es-sphere-view" shows 3es being used to visualise the progressive tesselation of a sphere from an icosahedron. This is a non-real time algorithm, so the term "frame" is used to loosely decribe a single step in the algorithm. Each frame outlines the face being considered in red and the tesselation to be effected in cyan. Once each face has been processed, the higher resolution sphere is shown before starting on the next iteration.    

@image html images/usecase/sphere-anim.gif "Sphere Tessellation"

@subsection subusecase1 Algorithm Debugging Usage Case
This use case looks at a complex 3D collision and intersection algorithm. Part of the algorithm involves intersecting sets of tetrahedra against a triangle mesh. Due to the high numbers of elements involved, this code was ported to GPU from code previously validated on CPU. The initial GPU results did not agree with the CPU code, so 3<sup>rd</sup> Eye Scene was used to visualise the data set being operated on and the results. The image below shows one such frame from the recorded output.

@image html images/usecase/frame245-wrong-bounds.png "Frame 245"

In this frame we see the following elements:
- Existing triangles in white.
- Tetrahedra in green.
- Triangles used to generate the tetrahedra in cyan (wireframe).
- A transparent bounding box (green tint).
- The set of candidate triangles from the existing set highlighted in blue.

This frame immediately shows an issue with the bounding box used. The bounds should tightly fix the tetrahedra. However, The box clearly extents too far to the right and bottom of the image and not far enough to the left.

Next, we see an image of the supposedly intersected triangles, highlighted in red. The insert in the top left corder identifies where these triangles appear in the original image.

@image html images/usecase/frame247-wrong-triangles.png "Intersected triangles"

Clearly these triangles are nowhere near the tetrahedra and are not intersecting. The errant bounds is the primary clue and it was quickly discovered that vertex data was being accessed with the wrong stride, moving one floating point element per vertex rather than three per vertex. This issue was very quickly identified and solved as a direct consequence of using 3<sup>rd</sup> Eye Scene to record and visualise the output. 

@subsection subusecase2 Debugging Games
The 3<sup>rd</sup> Eye Scene viewer is built on Unity 3D, which is primarily a game engine. Most game logic and apparent behaviour relies heavily on "smoke and mirrors" thinking to give the user the illusion of a consistent world while maintaining high, real-time performance and low implementation cost. The physics world, for example, is invariably a coarse representation of the rendered geometry. Less geometry is much faster for physics calculations. AI is typically implemented using simple state machines and behaviour trees and it is not always immediately apparent what the AI is doing or why certain decisions are made. Visualisation of physics geometry and AI state is common practise during game development.

Note that proprietary physics viewers are commonly available with commercial physics viewer such as <a href="https://www.havok.com/physics/">Havok Physics</a> and <a href="https://developer.nvidia.com/physx-sdk">PhysX</a>. Unity itself its own physics viewer in development. 3<sup>rd</sup> Eye Scene is not intended to compete with these tool as they are often tailored to exposing low level details only available deep in the internals of the physics engine. Instead, 3<sup>rd</sup> Eye Scene is inspired by such tools and can be used to expose game play and AI related information in a similar fashion.

Below is an image showing the physics geometry and AI state of the Unity <a href="https://www.assetstore.unity3d.com/en/#!/content/12175">Angry Bots</a> sample.

@image html images/usecase/bots.gif "Angry Bots"

By playing back the recording it becomes possible to see things which are not otherwise immediately apparent:
- Show AI state changes such as when becoming aware of the player.
- Visualise single frame events such as firing.
- Show AI timers.

*/
