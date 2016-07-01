using System;
using System.Collections.Generic;

public class MultiMap<K, V>: Dictionary <K, List<V>>
{
	public MultiMap ()
	{
	}

	public void Add (K key, IEnumerable<V> values) {
		if (ContainsKey (key))
			this [key].AddRange (values);
		else
			this [key] = new List<V> ();
	}

	public void Add (K key, V value) {
		if (!ContainsKey (key))
			this [key] = new List <V> ();

		this [key].Add (value);
	}


	public void Remove (K key, V value) {
		this [key].Remove (value);

		if (this [key].Count == 0)
			Remove (key);
	}
}