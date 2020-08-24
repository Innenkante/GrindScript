using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class Enemy : ConvertedObject
    {
        public EnemyTypes Type;

        public Enemy(object originalType) : base(originalType)
        {
            Type = (EnemyTypes)_originalType.enType;
        }
    }
}
