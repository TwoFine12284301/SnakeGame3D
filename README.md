=====================================================================================

These SnakeGame was build upon the work of haili1234. https://github.com/haili1234/UnitySnakeGame.git

=====================================================================================

MISSION STATEMENT
	Our mission is to improve the classic snake experience by leveraing #D spatial awareness and
	intuitive user inteface. We am to provide a smooth transition for playyers moving between 2D
	and 3D environemnts by efficient visual feedback. 


DOCUMENTATION
	The edge manager system a UI utility created using Unity designed to guide the player
	towards off screen objects. It calculates the position of the target relative to the camara
	diplays a pulsating arrow ponting on the edges of the screen. 
	
	
	The GridCube.cs is the backbone of the snake world. It manages the physical appereances and
	metadeta. The surface noormal vector calculation is the core math function, calculating
	the normal vector of the cube's surface. After the vector is calculated it is used to spawn
	the objects on the outside.
	
	We use trig functions to crate animations without the overhead of the unity animation window
