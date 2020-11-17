using System.Collections.Generic;
using UnityEngine;

namespace Core.Pooling
{
   public class HidePoolObject : MonoBehaviour 
   {
      public HidePool.Pool owner;

      // intrusive list for pool and user of pool
      public LinkedListNode<HidePoolObject> pool = null;
   }
}


