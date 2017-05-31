using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Complex {

	[SerializeField]
	private float x;
	public float Re {
		get{ return x;}
		set{ x = value;}
	}

	[SerializeField]
	private float y;
	public float Im {
		get{ return y;}
		set{ y = value;}
	}

	public Complex Conj{
		get{ return new Complex (x, -y);}
	}

	public Complex(float _x, float _y){
		this.x = _x;
		this.y = _y;
	}

	public Vector2 Position{
		get{return new Vector2 (x, y);}
	}

	public Vector3 getPosition(float t, float pow){
		return new Vector3 (x * pow, y * pow, t);
	}

	public static Complex operator+ (Complex z, Complex w)
	{
		return new Complex(z.Re + w.Re, z.Im + w.Im);
	}

	public static Complex operator+ (Complex z, float w)
	{
		return new Complex(z.Re + w, z.Im);
	}

	public static Complex operator- (Complex z, Complex w)
	{
		return new Complex(z.Re - w.Re, z.Im - w.Im);
	}


	public static Complex operator- (Complex z, float w)
	{
		return new Complex(z.Re - w, z.Im);
	}

	public static Complex operator* (Complex z, Complex w)
	{
		return new Complex(z.Re * w.Re - z.Im * w.Im, z.Re * w.Im + z.Im * w.Re);
	}
}
