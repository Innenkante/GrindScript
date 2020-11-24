using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class LocalGame : ConvertedObject
    {
        public LocalGame(object originalType) : base(originalType)
        {
        }

        public dynamic GetUnderlayingGame()
            => _originalObject;

        public int GetCurrentFloor()
            => _originalObject.xGameSessionData.xRogueLikeSession.iCurrentFloor;

        public dynamic GetContentManager()
            => _originalObject.Content;
    }
}
