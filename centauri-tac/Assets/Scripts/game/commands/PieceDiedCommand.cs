using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class PieceDiedCommand : Command
    {
        [Inject]
        public MinionModel pieceDied { get; set; }

        [Inject]
        public MinionsModel minionsModel { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            minionsModel.minions.Remove(pieceDied);
            GameObject.Destroy(pieceDied.gameObject, 0.1f);
        }
    }
}

