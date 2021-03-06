﻿using UnityEngine;
using System.Collections;

public class WorldManager : MonoBehaviour {
	
	//
	public float total = 0;
	float[] depth_new;
	//

	//Our renderers that'll make the top of the water and land visible
	LineRenderer LandBody;
	LineRenderer WaterBody;

	//Our physics arrays, we are only concerned with stuff in the r direction
	float[] LandSurfaceHeight;
	float[] WaterSurfaceHeight;
	float[] LandSurfaceHeight_velocities;
	float[] WaterSurfaceHeight_velocities;
	float[] LandSurfaceHeight_accelerations;
	float[] WaterSurfaceHeight_accelerations;

	//Vectors for the bottomof the water and land 
	float[] LandBottom;
	float[] WaterBottom;

	//This is for the circular geometry
	float[] theta; 

	//Our meshes and colliders, need colliders for each line renderer and mesh array for land and water
	GameObject[] meshobjects;
	GameObject[] Land_colliders;
	GameObject[] Water_colliders;
	Mesh[] WaterMeshes;
	Mesh[] LandMeshes;


	//Our particle system
	public GameObject splash;

	//The material we're using for the top of the water and land
	public Material Land_mat;
	public Material Water_mat;

	//The GameObject we're using for a mesh, I think the same one can be use for each
	public GameObject Land_mesh;
	public GameObject Water_mesh;

	//All our constants
	const float springconstant = 0.06f;
	const float damping = 0.08f;
	const float spread = 0.1f;

	//The depth of the fluid, i.e. the difference between LandSurfaceHeight and WaterSurfaceHeight. Is it neccessary to store this?
	float[] depth;

	//Set the number of nodes to be used
	int nodecount = 800;

	//Add a counter so that the land only moves for a short while after impact, this will do for now
	public int impactCount;

	void Start()
	{
		//Assign the size of the arrays
		LandSurfaceHeight = new float[nodecount];
		WaterSurfaceHeight = new float[nodecount];
		LandSurfaceHeight_velocities = new float[nodecount];
		WaterSurfaceHeight_velocities = new float[nodecount];
		LandSurfaceHeight_accelerations = new float[nodecount];
		WaterSurfaceHeight_accelerations = new float[nodecount];
		depth = new float[nodecount];
		LandBottom = new float[nodecount];
		WaterBottom = new float[nodecount];

		//Declare our mesh arrays
		//meshobjects = new GameObject[nodecount];
		LandMeshes = new Mesh[nodecount];
		WaterMeshes = new Mesh[nodecount];

		//And also theta
		theta = new float[nodecount];
		for (int i = 0; i < nodecount; i++) {
			theta [i] = 2 * i * Mathf.PI / (nodecount);
		}

//		GameObject LandRenderer = new GameObject();
//		LandRenderer.name = "LandRenderer";

//		GameObject WaterRenderer = new GameObject();
//		WaterRenderer.name = "WaterRenderer";

		//Add our line renderers and set them up, it seems like passing them to a function to be initiated does not work
	//	LandBody = LandRenderer.AddComponent<LineRenderer>();
//		LandBody.material = Land_mat;
//		LandBody.material.renderQueue = 1000;
//		LandBody.SetVertexCount(nodecount+1);
//		LandBody.SetWidth(0.1f, 0.1f);

//		WaterBody = WaterRenderer.AddComponent<LineRenderer>();
//		WaterBody.material = Water_mat;
//		WaterBody.material.renderQueue = 1000;
//		WaterBody.SetVertexCount(nodecount+1);
//		WaterBody.SetWidth(0.1f, 0.1f);

		RandomSurface ();

		//Now set the initial positions of the surfaces
		for (int i = 0; i < nodecount; i++)
		{
			if (i % 37 == 0) {
				depth [i] = 40;
			} else {
				depth [i] = 0;
			}

		//	LandSurfaceHeight[i] = 30;//+Mathf.Cos(i); //Because why not

			//Set the bottomof the Land so that it has a constant thickness
			LandBottom[i] = LandSurfaceHeight[i] - 10;
			WaterBottom [i] = 0;

			//This is defined as depth of water plus height of land 
			WaterSurfaceHeight[i] = depth [i] + LandSurfaceHeight[i];

			LandSurfaceHeight_velocities[i] = 0;
			WaterSurfaceHeight_velocities[i] = 0;
			LandSurfaceHeight_accelerations[i] = 0;
			WaterSurfaceHeight_accelerations[i] = 0;

			//Set the line renderers
//			LandBody.SetPosition (i, new Vector3 (LandSurfaceHeight [i] * Mathf.Cos (theta [i]), 
//				LandSurfaceHeight [i] * Mathf.Sin (theta [i]), 0));
//			WaterBody.SetPosition (i, new Vector3 (WaterSurfaceHeight [i] * Mathf.Cos (theta [i]), 
//				WaterSurfaceHeight [i] * Mathf.Sin (theta [i]), 2));
		}

		//Let's add some extra bits to the land and in general do more interesting stuff with that

	//	for (int i = 200; i < nodecount-200; i++) {
	//		depth [i] = 10;
//			LandSurfaceHeight [i] += 5;
//		}
	/*	for (int i = 0; i < 30; i++) {
			LandSurfaceHeight [nodecount-200+i] += (30-i)/3;
		}
		for (int i = 0; i < 30; i++) {
			LandSurfaceHeight [170+i] += i/3;
		}
		for (int i = nodecount-150; i < nodecount-125; i++) {
			LandSurfaceHeight [i] -= 5;
		}
*/

	


		//Periodic boundaries so the end  of the line is the start
//		LandBody.SetPosition (nodecount, new Vector3 (LandSurfaceHeight [0] * Mathf.Cos (theta [0]), 
	//		LandSurfaceHeight [0] * Mathf.Sin (theta [0]), 0));
//		WaterBody.SetPosition (nodecount, new Vector3 (WaterSurfaceHeight [0] * Mathf.Cos (theta [0]), 
//			WaterSurfaceHeight [0] * Mathf.Sin (theta [0]), 2));

		Land_colliders = new GameObject[nodecount];
		Water_colliders = new GameObject[nodecount];

		for (int i = 0; i < nodecount; i++) {

			//Create our colliders, set them be our child
			Water_colliders [i] = new GameObject ();
			Water_colliders [i].name = "WaterTrigger";
			Water_colliders [i].AddComponent<CircleCollider2D> ();
			Water_colliders [i].transform.parent = transform;

			Land_colliders [i] = new GameObject ();
			Land_colliders [i].name = "LandTrigger";
			Land_colliders [i].AddComponent<CircleCollider2D> ();
			Land_colliders [i].transform.parent = transform;

			//Set the position and scale to the correct dimensions, might need to look more closeley at the scale
			Water_colliders [i].transform.position = new Vector3 (WaterSurfaceHeight [i] * Mathf.Cos (theta [i]), WaterSurfaceHeight [i] * Mathf.Sin (theta [i]), 0);
			Water_colliders [i].transform.localScale = new Vector3 (1, 1, 1);

			Land_colliders [i].transform.position = new Vector3 (LandSurfaceHeight [i] * Mathf.Cos (theta [i]), LandSurfaceHeight [i] * Mathf.Sin (theta [i]), 0);
			Land_colliders [i].transform.localScale = new Vector3 (1, 1, 1);

			//Add a WaterDetector and make sure they're triggers
			Water_colliders [i].GetComponent<CircleCollider2D> ().isTrigger = true;
			Water_colliders [i].AddComponent<WaterDetector> ();
			Water_colliders [i].GetComponent<WaterDetector> ().index = i;
			Water_colliders [i].GetComponent<WaterDetector> ().type = "WaterTrigger";

			Land_colliders [i].GetComponent<CircleCollider2D> ().isTrigger = true;
			Land_colliders [i].AddComponent<WaterDetector> ();
			Land_colliders [i].GetComponent<WaterDetector> ().index = i;
			Land_colliders [i].GetComponent<WaterDetector> ().type = "LandTrigger";
		}


		//Spawn the land and then the water using the same function
		SpawnMeshes ( LandSurfaceHeight, LandBottom, 0, Land_mesh, Land_mat, LandMeshes );
		SpawnMeshes ( WaterSurfaceHeight, WaterBottom, 100, Water_mesh, Water_mat, WaterMeshes );
	}


	public void Splash( float brickVelocity, int index, float angle )
	{
		//Add the velocity of the falling object to the spring, it is towards the  origin 
		//WaterSurfaceHeight_velocities [index] -= brickVelocity/10.0f;

		//Now also move some fluid away from the centre of the impact
		if (index != 0 && index != nodecount-1) {
			float displace = depth [index];
			if (theta [index] > angle) {
				depth [index] -= displace;
				depth [index + 1] += displace;
			} else {
				depth [index] -= displace;
				depth [index - 1] += displace;
			}
		}

		//Set the lifetime of the particle system.
		float lifetime = 0.93f + Mathf.Abs (brickVelocity) * 0.07f;

		//Set the splash to be between two values in Shuriken by setting it twice.
		splash.GetComponent<ParticleSystem> ().startSpeed = 8 + 2 * Mathf.Pow (Mathf.Abs (brickVelocity), 0.5f);
		splash.GetComponent<ParticleSystem> ().startSpeed = 9 + 2 * Mathf.Pow (Mathf.Abs (brickVelocity), 0.5f);
		splash.GetComponent<ParticleSystem> ().startLifetime = lifetime;
		splash.GetComponent<ParticleSystem> ().gravityModifier = 0;
		//splash.GetComponent<ParticleSystem> ().forceOverLifetime = Vector3 (Mathf.Cos (theta [index]), Mathf.Sin (theta [index]), 0);
		//splash.GetComponent<ParticleSystem> ().GetComponent<ConstantForce>() = Vector3 (Mathf.Cos (theta [index]), Mathf.Sin (theta [index]), 0);


		//Set the correct position of the particle system.
		Vector3 position = new Vector3 (WaterSurfaceHeight [index] * Mathf.Cos (theta [index]), WaterSurfaceHeight [index] * Mathf.Sin (theta [index]), 5);
	
		//This line aims the splash towards the middle. Only use for small bodies of water:
		Quaternion rotation = Quaternion.LookRotation (new Vector3 (WaterSurfaceHeight [index] * Mathf.Cos (theta [index]), WaterSurfaceHeight [index] * Mathf.Sin (theta [index]), 5));

		//Create the splash and tell it to destroy itself.
		GameObject splish = Instantiate (splash, position, rotation) as GameObject;
		Destroy (splish, lifetime + 0.3f);

		//Change surface here instead of in update
		//WaterSurfaceHeight [index] += 3.0f * brickVelocity;
//		WaterBody.SetPosition (index, new Vector3 (WaterSurfaceHeight [index] * Mathf.Cos (theta [index]), WaterSurfaceHeight [index] * Mathf.Sin (theta [index]), 2));			
//		WaterBody.SetPosition (nodecount, new Vector3 (WaterSurfaceHeight [0] * Mathf.Cos (theta [0]), WaterSurfaceHeight [0] * Mathf.Sin (theta [0]), 2));
		UpdateMeshes ( WaterSurfaceHeight, WaterBottom, Water_colliders, 100, WaterMeshes );
	}

	public void Impact ( float mass, int index, float angle ){

		//LandSurfaceHeight [index] -= 1.0f;
		//LandBottom [index] -= 1.0f;

		// The impact crater, the size depends on the mass
		int size = 2*(int)mass; 
		//print (size);
		for (int i = index - size / 2; i < index + size / 2; i++) {

			// Normalise to interval (0,1)
			float iNorm = (float)(i - index + size / 2) / (size - 1); 
			float mo = iNorm * (1 - iNorm) * Mathf.Exp (-50.0f * Mathf.Pow (iNorm - 0.5f, 2));//(i - start - baseSize / 2) * (i - start - baseSize / 2));

			// Deal with the boundary
			if (i < nodecount && i >= 0) {
				LandSurfaceHeight [i] -= size * mo;
				LandBottom [i] -= size * mo;
			} else if (i < 0) {
				LandSurfaceHeight [nodecount + i] -= size * mo;
				LandBottom [nodecount + i] -= size * mo;
			} else {
				LandSurfaceHeight [i - nodecount] -= size * mo;
				LandBottom [i - nodecount] -= size * mo;
			}
			print (iNorm);
		}
					
	
		UpdateMeshes (LandSurfaceHeight, LandBottom, Land_colliders, 0, LandMeshes);
	
	}

	public void SpawnMeshes ( float[] r_positions, float[] bottom, int z, GameObject mesh, Material mat, Mesh[] meshes )
	{
		meshobjects = new GameObject[nodecount];

		//Setting the meshes now:
		for (int i = 0; i < nodecount - 1; i++) {
			//Make the mesh
			meshes [i] = new Mesh ();

			//Create the corners of the mesh, I want all the meshes to extend to the origin so this is maybe a bit stupid
			Vector3[] Vertices = new Vector3[4];
			Vertices [0] = new Vector3 (r_positions [i] * Mathf.Cos (theta [i]), r_positions [i] * Mathf.Sin (theta [i]), z);
			Vertices [1] = new Vector3 (r_positions [i + 1] * Mathf.Cos (theta [i + 1]), r_positions [i + 1] * Mathf.Sin (theta [i + 1]), z);
			Vertices [2] = new Vector3 (bottom [i] * Mathf.Cos (theta [i]), bottom [i] * Mathf.Sin (theta [i]), z);
			Vertices [3] = new Vector3 (bottom [i + 1] * Mathf.Cos (theta [i+1]), bottom [i + 1] * Mathf.Sin (theta [i + 1]), z);

			//Set the UVs of the texture
			Vector2[] UVs = new Vector2[4];
			UVs [0] = new Vector2 (0, 1);
			UVs [1] = new Vector2 (1, 1);
			UVs [2] = new Vector2 (0, 0);
			UVs [3] = new Vector2 (1, 0);

			//Set where the triangles should be.
			int[] tris = new int[6] { 0, 1, 3, 3, 2, 0 };

			//Add all this data to the mesh.
			meshes [i].vertices = Vertices;
			meshes [i].uv = UVs;
			meshes [i].triangles = tris;

			//Create a holder for the mesh, set it to be the manager's child
			meshobjects [i] = Instantiate (mesh, Vector3.zero, Quaternion.identity) as GameObject;
			meshobjects [i].GetComponent<MeshFilter> ().mesh = meshes [i];
			meshobjects [i].transform.parent = transform;
		}
			
		//Make a mesh to connect the two ends up
		meshes [nodecount - 1] = new Mesh ();

		//Create the corners of the mesh, again this is stupid
		Vector3[] ConnectVertices = new Vector3[4];
		ConnectVertices [0] = new Vector3 (r_positions [nodecount - 1] * Mathf.Cos (theta [nodecount - 1]), r_positions [nodecount - 1] * Mathf.Sin (theta [nodecount - 1]), z);
		ConnectVertices [1] = new Vector3 (r_positions [0] * Mathf.Cos (theta [0]), r_positions [0] * Mathf.Sin (theta [0]), z);
		ConnectVertices [2] = new Vector3 (bottom [nodecount - 1] * Mathf.Cos (theta [nodecount - 1]), bottom [nodecount - 1] * Mathf.Sin (theta [nodecount - 1]), z);
		ConnectVertices [3] = new Vector3 (bottom [0] * Mathf.Cos (theta [0]), bottom [0] * Mathf.Sin (theta [0]), z);

		//Set the UVs of the texture
		Vector2[] ConnectUVs = new Vector2[4];
		ConnectUVs [0] = new Vector2 (0, 1);
		ConnectUVs [1] = new Vector2 (1, 1);
		ConnectUVs [2] = new Vector2 (0, 0);
		ConnectUVs [3] = new Vector2 (1, 0);

		//Set where the triangles should be.
		int[] Connecttris = new int[6] { 0, 1, 3, 3, 2, 0 };

		//Add all this data to the mesh.
		meshes [nodecount - 1].vertices = ConnectVertices;
		meshes [nodecount - 1].uv = ConnectUVs;
		meshes [nodecount - 1].triangles = Connecttris;

		//Create a holder for the mesh, set it to be the manager's child
		meshobjects [nodecount - 1] = Instantiate (mesh, Vector3.zero, Quaternion.identity) as GameObject;
		meshobjects [nodecount - 1].GetComponent<MeshFilter> ().mesh = meshes [nodecount - 1];
		meshobjects [nodecount - 1].transform.parent = transform;
	}


	//Same as the code from in the meshes before, set the new mesh positions, could probably combine the two things so all the mesh stuff is in one functiom
	void UpdateMeshes ( float[] r_positions, float[] bottom, GameObject[] colliders, int z, Mesh[] meshes ){
		for (int i = 0; i < nodecount-1; i++)
		{

			Vector3[] Vertices = new Vector3[4];
			Vertices [0] = new Vector3 (r_positions [i] * Mathf.Cos (theta [i]), r_positions [i] * Mathf.Sin (theta [i]), z);
			Vertices [1] = new Vector3 (r_positions [i + 1] * Mathf.Cos (theta [i + 1]), r_positions [i + 1] * Mathf.Sin (theta [i + 1]), z);
			Vertices [2] = new Vector3 (bottom [i] * Mathf.Cos (theta [i]), bottom [i] * Mathf.Sin (theta [i]), z);
			Vertices [3] = new Vector3 (bottom [i + 1] * Mathf.Cos (theta [i + 1]), bottom [i + 1] * Mathf.Sin (theta [i + 1]), z);

			meshes[i].vertices = Vertices;
	
			//Also update the colliders
			colliders [i].transform.position = new Vector3 (r_positions [i] * Mathf.Cos (theta [i]), r_positions [i] * Mathf.Sin (theta [i]), 0);

		}

		//Connect the wrapped around bits
		Vector3[] ConnectVertices = new Vector3[4];
		ConnectVertices [0] = new Vector3 (r_positions [nodecount - 1] * Mathf.Cos (theta [nodecount - 1]), r_positions [nodecount - 1] * Mathf.Sin (theta [nodecount - 1]), z);
		ConnectVertices [1] = new Vector3 (r_positions [0] * Mathf.Cos (theta [0]), r_positions [0] * Mathf.Sin (theta [0]), z);
		ConnectVertices [2] = new Vector3 (bottom [nodecount - 1] * Mathf.Cos (theta [nodecount - 1]), bottom [nodecount - 1] * Mathf.Sin (theta [nodecount - 1]), z);
		ConnectVertices [3] = new Vector3 (bottom [0] * Mathf.Cos (theta [0]), bottom [0] * Mathf.Sin (theta [0]), z);

		meshes[nodecount-1].vertices = ConnectVertices;

		//Also update the colliders
		colliders[nodecount-1].transform.position = new Vector3(r_positions[nodecount-1]*Mathf.Cos(theta[nodecount-1]), r_positions[nodecount-1]*Mathf.Sin(theta[meshes.Length-1]), 0);



	}

	//Called regularly by Unity
	void FixedUpdate()
	{

		// Only motion in the r direction is explicitly considered, also these may not need to be built dynamically if using nodecount
		//(this applies to all the arrays). Thses are all temporary 
		float[] WaterSurfaceHeight_new = new float[nodecount];	
		float[] WaterSurfaceHeight_velocities_new = new float[nodecount];	
		float[] WaterSurfaceHeight_accelerations_new = new float[nodecount];
		depth_new = new float[nodecount];



		/*
		for (int i = 0; i < nodecount; i++) { 
			depth_new[i] = depth[i];
		}

		

		for (int i = 0; i < nodecount; i++) {
			total += depth_new [i];
		}
		*/	


		// First allow for diffusion of the fluid and use this to work out the equilibrium position of each spring and do it abunch of times
		// in each update like the original code, like taking 8 timesteps each time
		for (int j = 0; j < 8; j++) {
			
			for (int i = 0; i < nodecount; i++) { 
				depth_new[i] = depth[i];
			}
			//may not need to define depth_new if height change stored as float?
			for (int i = 0; i < nodecount; i++) {
				if (depth [i] > 0) {
					if (i == 0) {
						if (depth [i] + LandSurfaceHeight [i] > depth [i + 1] + LandSurfaceHeight [i + 1]) {
							depth_new [i + 1] += (depth [i] + LandSurfaceHeight [i] - depth [i + 1] - LandSurfaceHeight [i + 1]) / 3.0f;
							depth_new [i] -= (depth [i] + LandSurfaceHeight [i] - depth [i + 1] - LandSurfaceHeight [i + 1]) / 3.0f;
						}
						if (depth [i] + LandSurfaceHeight [i] > depth [nodecount - 1] + LandSurfaceHeight [nodecount - 1]) {
							depth_new [nodecount - 1] += (depth [i] + LandSurfaceHeight [i] - depth [nodecount - 1] - LandSurfaceHeight [nodecount - 1]) / 3.0f;
							depth_new [i] -= (depth [i] + LandSurfaceHeight [i] - depth [nodecount - 1] - LandSurfaceHeight [nodecount - 1]) / 3.0f;  
						}
					} else if (i == nodecount - 1) {
						if (depth [i] + LandSurfaceHeight [i] > depth [0] + LandSurfaceHeight [0]) {
							depth_new [0] += (depth [i] + LandSurfaceHeight [i] - depth [0] - LandSurfaceHeight [0]) / 3.0f;
							depth_new [i] -= (depth [i] + LandSurfaceHeight [i] - depth [0] - LandSurfaceHeight [0]) / 3.0f;
						}
						if (depth [i] + LandSurfaceHeight [i] > depth [i - 1] + LandSurfaceHeight [i - 1]) {
							depth_new [i - 1] += (depth [i] + LandSurfaceHeight [i] - depth [i - 1] - LandSurfaceHeight [i - 1]) / 3.0f;
							depth_new [i] -= (depth [i] + LandSurfaceHeight [i] - depth [i - 1] - LandSurfaceHeight [i - 1]) / 3.0f;     
						} 
					} else {				
						if (depth [i] + LandSurfaceHeight [i] > depth [i + 1] + LandSurfaceHeight [i + 1]) {
							depth_new [i + 1] += (depth [i] + LandSurfaceHeight [i] - depth [i + 1] - LandSurfaceHeight [i + 1]) / 3.0f;
							depth_new [i] -= (depth [i] + LandSurfaceHeight [i] - depth [i + 1] - LandSurfaceHeight [i + 1]) / 3.0f;
						}
						if (depth [i] + LandSurfaceHeight [i] > depth [i - 1] + LandSurfaceHeight [i - 1]) {
							depth_new [i - 1] += (depth [i] + LandSurfaceHeight [i] - depth [i - 1] - LandSurfaceHeight [i - 1]) / 3.0f;
							depth_new [i] -= (depth [i] + LandSurfaceHeight [i] - depth [i - 1] - LandSurfaceHeight [i - 1]) / 3.0f;     
						}
					}
				}
			}

			for (int i = 0; i < nodecount; i++) { 
				depth[i] = depth_new[i];
			}

		}
		total = 0;

		for (int i = 0; i < nodecount; i++) {
			total += depth_new [i];
		}
			


		//Update the depth all at once
	//	for (int i = 0; i < nodecount; i++) { 
	//		depth[i] = depth_new[i];
	//	}
		//depth = depth_new;// doing this makes them the same thing?

		total = 0;
		/*
		for(int i = 0; i<nodecount; i++){ total += depth [i];
			WaterSurfaceHeight[i] = LandSurfaceHeight[i] + depth[i];
			WaterBody.SetPosition(i, new Vector3(WaterSurfaceHeight[i]*Mathf.Cos(theta[i]), WaterSurfaceHeight[i]*Mathf.Sin(theta[i]), 2));

				} 

		WaterBody.SetPosition(nodecount, new Vector3(WaterSurfaceHeight[0]*Mathf.Cos(theta[0]), WaterSurfaceHeight[0]*Mathf.Sin(theta[0]), 2));
*/



		for (int i = 0; i < nodecount; i++) {
			//Spherical form	

			//			float ratio = Mathf.Sqrt(Mathf.Pow (r_positions[i],2)/Mathf.Pow (theta_positions[i],2) + 1)/Mathf.Sqrt(Mathf.Pow (r_positions[i],2) + Mathf.Pow (theta_positions[i],2)); 
			//			float xforce = 5f*springconstant*(r_positions[i] + r_floor[i]) + r_velocities[i]*damping; //*r_positions[i]*ratio*damping;			
			//			float yforce = 5f*springconstant*(theta_positions[i] + theta_floor[i]) + theta_velocities[i]*damping;//*r_positions[i]*ratio*damping;
			//Gravity don't work how I want

			//Make it some surface is is at lowest point if there is no water here, i.e. level with lowest water next to it 
			if (i != 0 && i != nodecount - 1) {
				if (depth [i] > 0.0001) {
					float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i] + depth [i])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
					WaterSurfaceHeight_accelerations [i] = -r_force;

					WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
					WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
				} else {
					//Find local land gradient, sort of
					if (LandSurfaceHeight [i + 1] - LandSurfaceHeight [i - 1] > 0) {
						float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i - 1] + depth [i - 1])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
						WaterSurfaceHeight_accelerations [i] = -r_force;

						WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
						WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
					} else {
						float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i + 1] + depth [i + 1])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
						WaterSurfaceHeight_accelerations [i] = -r_force;

						WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
						WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
					}
				} //Take accountof the periodic boundary
			} else if (i != 0) {
				if (depth [i] > 0.0001) {
					float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i] + depth [i])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
					WaterSurfaceHeight_accelerations [i] = -r_force;

					WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
					WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
				} else {
					//Find local land gradient, sort of
					if (LandSurfaceHeight [0] - LandSurfaceHeight [i - 1] > 0) {
						float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i - 1] + depth [i - 1])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
						WaterSurfaceHeight_accelerations [i] = -r_force;

						WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
						WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
					} else {
						float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [0] + depth [0])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
						WaterSurfaceHeight_accelerations [i] = -r_force;

						WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
						WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
					}
				}
			} else {
				if (depth [i] > 0.0001) {
					float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i] + depth [i])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
					WaterSurfaceHeight_accelerations [i] = -r_force;

					WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
					WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
				} else {
					//Find local land gradient, sort of
					if (LandSurfaceHeight [i + 1] - LandSurfaceHeight [nodecount - 1] > 0) {
						float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [nodecount - 1] + depth [nodecount - 1])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
						WaterSurfaceHeight_accelerations [i] = -r_force;

						WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
						WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
					} else {
						float r_force = springconstant * (WaterSurfaceHeight [i] - (LandSurfaceHeight [i + 1] + depth [i + 1])) + WaterSurfaceHeight_velocities [i] * damping; // + 0.01f*r_positions[i];
						WaterSurfaceHeight_accelerations [i] = -r_force;

						WaterSurfaceHeight [i] += WaterSurfaceHeight_velocities [i];
						WaterSurfaceHeight_velocities [i] += WaterSurfaceHeight_accelerations [i];
					}
				}
			}
		}

		/*


		//Now we store the difference in heights:
		float[] leftDeltas = new float[WaterSurfaceHeight.Length];
		float[] rightDeltas = new float[WaterSurfaceHeight.Length];

		//We make 8 small passes for fluidity:
		for (int j = 0; j < 8; j++)
		{
			for (int i = 0; i < WaterSurfaceHeight.Length; i++)
			{
				//We check the heights of the nearby nodes, adjust velocities accordingly, record the height differences
				if (i > 0)
				{
					leftDeltas[i] = spread * (WaterSurfaceHeight[i] - WaterSurfaceHeight[i - 1]);
					WaterSurfaceHeight_velocities[i - 1] += leftDeltas[i];
				}
				if (i == 0)
				{
					leftDeltas[i] = spread * (WaterSurfaceHeight[i] - WaterSurfaceHeight[WaterSurfaceHeight.Length - 1]);
					WaterSurfaceHeight_velocities[WaterSurfaceHeight.Length - 1] += leftDeltas[i];
				}
				if (i < WaterSurfaceHeight.Length - 1)
				{
					rightDeltas[i] = spread * (WaterSurfaceHeight[i] - WaterSurfaceHeight[i + 1]);
					WaterSurfaceHeight_velocities[i + 1] += rightDeltas[i];
				}
				if (i == WaterSurfaceHeight.Length - 1)
				{
					rightDeltas[i] = spread * (WaterSurfaceHeight[i] - WaterSurfaceHeight[0]);
					WaterSurfaceHeight_velocities[0] += rightDeltas[i];
				}
			}

			//Now we apply a difference in position
		for (int i = 0; i < WaterSurfaceHeight.Length; i++)
			{
				if (i > 0)
					WaterSurfaceHeight[i-1] += leftDeltas[i];
				if (i < WaterSurfaceHeight.Length - 1)
					WaterSurfaceHeight[i + 1] += rightDeltas[i];
			}
		}

	*/

		for(int i = 0; i<nodecount; i++){ total += depth [i];
			//	WaterSurfaceHeight[i] = LandSurfaceHeight[i] + depth[i];
//			WaterBody.SetPosition(i, new Vector3(WaterSurfaceHeight[i]*Mathf.Cos(theta[i]), WaterSurfaceHeight[i]*Mathf.Sin(theta[i]), 2));

		} 

//		WaterBody.SetPosition(nodecount, new Vector3(WaterSurfaceHeight[0]*Mathf.Cos(theta[0]), WaterSurfaceHeight[0]*Mathf.Sin(theta[0]), 2));




		UpdateMeshes ( WaterSurfaceHeight, WaterBottom, Water_colliders, 100, WaterMeshes );




		/*
















		//		if (impactCount > 1) 
		//		{
		//			float[] r_positions_new = new float[r_positions.Length];	
		//			float[] theta_positions_new = new float[theta_positions.Length];
		//		
		//			//		r_positions_new = r_positions;		
		//			//		theta_positions_new = theta_positions;
		//		
		//			//Here we use the Euler method to handle all the physics of our springs:
		//			for (int i = 0; i < r_positions.Length; i++) {
		//				//Spherical form	
		//			
		//				float r_force = springconstant * (r_positions [i] - 2.0f * r_floor [i]) + r_velocities [i] * damping;
		//				r_accelerations [i] = -r_force;
		//
		//				r_positions [i] += r_velocities [i];
		//				r_velocities [i] += r_accelerations [i];
		//			
		//				//			Body.SetPosition(i, new Vector3(r_positions_new[i], theta_positions_new[i], z));
		//				Body.SetPosition (i, new Vector3 (r_positions [i] * Mathf.Cos (theta_positions [i]), r_positions [i] * Mathf.Sin (theta_positions [i]), z));
		//			
		//			}
		//		
		//			Body.SetPosition (r_positions.Length, new Vector3 (r_positions [0] * Mathf.Cos (theta_positions [0]), r_positions [0] * Mathf.Sin (theta_positions [0]), z));
		//		
		//		
		//			//Finally we update the meshes to reflect this
		//			UpdateMeshes ();
		//			impactCount--;
		//		}
*/	}

	void OnTriggerStay2D(Collider2D Hit)
	{
		//Bonus exercise. Fill in your code here for making things float in your water.
		//You might want to even include a buoyancy constant unique to each object!
	}

	void RandomSurface()
	{
		float k = 0;
		int count = 0;
		bool m_up = false;
		bool m_down = false;
		bool o_up = false;
		bool o_down = false;
		float height = Random.Range (150, 160);


		while (count < nodecount) {

			// Make surface a little uneven
			//float test = Random.Range (0.0f, 1.0f);
			//if (test < 0.95) {

			//Make mountains, randomly increase gradient for a little while then decrease it again	
			float test = Random.Range (0.0f, 0.8f);

			if (test > 0.95) {
				m_up = true;
				m_down = false;
				o_up = false;
				o_down = false;
			} else if (test > 0.9) {
				m_up = false;
				m_down = true;
				o_up = false;
				o_down = false;
			} else if (test > 0.85) {
				m_up = false;
				m_down = false;
				o_up = true;
				o_down = false;
			}else if (test > 0.8) {
				m_up = false;
				m_down = false;
				o_up = false;
				o_down = true;
			}


			// If this number big enough enter mountain making mode
			if (m_up == true) {
				float u1 = Random.Range (0.0f, 1.0f); 
				float u2 = Random.Range (0.0f, 1.0f);
				float randStdNormal = 5.0f * Mathf.Sqrt (-2.0f * Mathf.Log (u1)) *
				                      Mathf.Sin (2.0f * Mathf.PI * u2); //random normal(0,1)

				LandSurfaceHeight [count] = height + randStdNormal;
				k = randStdNormal;

			} else if (m_down == true) {
				float u1 = Random.Range (0.0f, 1.0f); 
				float u2 = Random.Range (0.0f, 1.0f);
				float randStdNormal = -5.0f + 1.2f * Mathf.Sqrt (-2.0f * Mathf.Log (u1)) *
				                      Mathf.Sin (2.0f * Mathf.PI * u2); //random normal(0,1)
				
				LandSurfaceHeight [count] = height + randStdNormal;
				k = randStdNormal;
			} else {

				float u1 = Random.Range (0.0f, 1.0f); 
				float u2 = Random.Range (0.0f, 1.0f);
				float randStdNormal = 1.2f * Mathf.Sqrt (-2.0f * Mathf.Log (u1)) *
				                      Mathf.Sin (2.0f * Mathf.PI * u2); //random normal(0,1)

				LandSurfaceHeight [count] = height + randStdNormal;
				k = randStdNormal;
			}





				//} else {
				//		float u1 = Random.Range (0.0f, 1.0f); //these are uniform(0,1) random doubles
				//		float u2 = Random.Range (0.0f, 1.0f);
				//		float randStdNormal = 2.2f * Mathf.Sqrt (-2.0f * Mathf.Log (u1)) *
				//		                     Mathf.Sin (2.0f * Mathf.PI * u2); //random normal(0,1)

				//		LandSurfaceHeight [count + 1] = LandSurfaceHeight [count] + randStdNormal;
				//		k = randStdNormal;
				//}



			count++;
		}

		//Make mountains, randomly increase gradient for a little while then decrease it again



		// Now add a mountain, first choose size of base (how many nodes are covered) and steepness will depend on base size.
		// Add uniform random numbers with larger range closer to centre

		// Choose how many mountains
		int no = Random.Range(0,7);
		for (k = 0; k < no; k++) {
			float baseSize = Random.Range (10, nodecount / 10);
			int start = Random.Range (0, nodecount - 1);
			int repeat = Random.Range(1,3);
			for(int repeats = 0; repeats < repeat; repeats++) {
				start += Random.Range (-20, 20);
				for (int i = start; i < start + baseSize; i++) {
					// Deal with the boundary
					if (i < nodecount) {
						// Normalise to interval (0,1)
						float iNorm = (i - start) / (baseSize - 1); 
						float mo = iNorm * (1 - iNorm) * Mathf.Exp (-50.0f * Mathf.Pow (iNorm - 0.5f, 2));//(i - start - baseSize / 2) * (i - start - baseSize / 2));
						LandSurfaceHeight [i] += baseSize * mo + Random.Range (-0.5f, 1.0f); //((i - start) * (start + baseSize + 1 - i) * Mathf.Exp (-(i - start - baseSize / 2) * (i - start - baseSize / 2)) 
					} else {
						// Normalise to interval (0,1)
						float iNorm = (i - start) / (baseSize - 1); 
						float mo = iNorm * (1 - iNorm) * Mathf.Exp (-50.0f * Mathf.Pow (iNorm - 0.5f, 2));//(i - start - baseSize / 2) * (i - start - baseSize / 2));
						LandSurfaceHeight [i - nodecount] += baseSize * mo + Random.Range (-0.5f, 1.0f); //((i - start) * (start + baseSize + 1 - i) * Mathf.Exp (-(i - start - baseSize / 2) * (i - start - baseSize / 2)) 
					}
				}
			}
		}






		for (int j = 0; j < 2; j++) {
			float[] LandSurfaceHeight_old = new float[nodecount];
			// Average it out a bit
			for (int i = 0; i < nodecount; i++) {
				if (i == 0) {
					LandSurfaceHeight_old [i] = (LandSurfaceHeight [nodecount - 1] + LandSurfaceHeight [i] + LandSurfaceHeight [i + 1]) / 3.0f;
				} else if (i == nodecount - 1) {
					LandSurfaceHeight_old [i] = (LandSurfaceHeight [i - 1] + LandSurfaceHeight [i] + LandSurfaceHeight [0]) / 3.0f;
				} else {
					LandSurfaceHeight_old [i] = (LandSurfaceHeight [i - 1] + LandSurfaceHeight [i] + LandSurfaceHeight [i + 1]) / 3.0f;
				}
			}

			LandSurfaceHeight = LandSurfaceHeight_old;
		}


	}


}
	