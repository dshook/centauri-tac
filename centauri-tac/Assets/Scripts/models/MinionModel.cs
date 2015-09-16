
using UnityEngine;

namespace ctac
{
    public interface IMinionModel
    {
        GameObject gameObject { get; set; }
    }

    class MinionModel : IMinionModel
    {
        public GameObject gameObject { get; set; }
    }
}
