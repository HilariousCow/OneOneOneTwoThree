using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class ListExtensions  {
	
	public static void Push<T>(this List<T> stack, T obj)
	{
		stack.Add(obj);
	}
	
	public static void PushFront<T>(this List<T> stack, T obj)
	{
		stack.Insert(0, obj);
	}
	
	public static T Pop<T>(this List<T> stack)
	{
		if(stack.Count > 0)
		{
			T obj = stack[stack.Count - 1];
			stack.Remove(obj);
			return obj;
		} 
		else 
		{
			return default(T);
		}
		
	}
	
	public static T Peek<T>(this List<T> stack)
	{
		if(stack.Count > 0)
		{
			return stack[stack.Count - 1];
		} 
		else 
		{
			return default(T);
		}
	}
	
	public static T PeekFront<T>(this List<T> queue)
	{
		if(queue.Count > 0)
		{
			return queue[0];
		} 
		else
		{
			return default(T);
		}
		
	}
	
		
	public static T PopFront<T>(this List<T> queue)
	{
		if(queue.Count > 0){
			T obj = queue[0];
			queue.Remove(obj);
			return obj;
		} 
		else 
		{
			return default(T);
		}
	}
	
	
	//Only add an element if it's not already on the list
	public static bool AddUnique<T>(this List<T> list, T element)
	{
		if(!list.Contains(element))
		{
			list.Add(element);
		    return true;
		}
		else
		{
		    return false;
		}
	}
	
	//Only add an element if it's not already on the list
	public static bool RemoveUnique<T>(this List<T> list, T element)
	{
        if (list.Contains(element))
        {
            list.Remove(element);
            return true;
        }
        else
        {
            return false;
        }
	}
	
	
    public static IList<T> Clone<T>(this IList<T> listToClone) where T: ICloneable
    {
            return listToClone.Select(item => (T)item.Clone()).ToList();
    }

}
