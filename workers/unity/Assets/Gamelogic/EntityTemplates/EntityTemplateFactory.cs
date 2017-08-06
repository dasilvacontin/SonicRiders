using Assets.Gamelogic.Core;
using Improbable;
using Improbable.Core;
using Improbable.Player;
using Improbable.Unity.Core.Acls;
using Improbable.Worker;
using Quaternion = Improbable.Core.Quaternion;
using UnityEngine;
using Improbable.Unity.Entity;
using Assets.Gamelogic.Utils;

namespace Assets.Gamelogic.EntityTemplates
{
    public class EntityTemplateFactory : MonoBehaviour
    {
        public static Entity CreatePlayerCreatorTemplate()
        {
            var playerCreatorEntityTemplate = EntityBuilder.Begin()
                .AddPositionComponent(Improbable.Coordinates.ZERO.ToUnityVector(), CommonRequirementSets.PhysicsOnly)
                .AddMetadataComponent(entityType: SimulationSettings.PlayerCreatorPrefabName)
                .SetPersistence(true)
                .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
                .AddComponent(new Rotation.Data(new Quaternion(0, 0, 0, 0)), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new PlayerCreation.Data(), CommonRequirementSets.PhysicsOnly)
                .Build();

            return playerCreatorEntityTemplate;
        }

        public static Entity CreatePlayerTemplate(string clientId)
        {
			var clientWriteAccess = CommonRequirementSets.SpecificClientOnly (clientId);
            var playerTemplate = EntityBuilder.Begin()
                .AddPositionComponent(Improbable.Coordinates.ZERO.ToUnityVector(), clientWriteAccess)
                .AddMetadataComponent(entityType: SimulationSettings.PlayerPrefabName)
                .SetPersistence(false)
                .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Rotation.Data(UnityEngine.Quaternion.identity.ToNativeQuaternion()), clientWriteAccess)
                .AddComponent(new ClientAuthorityCheck.Data(), clientWriteAccess)
                .AddComponent(new ClientConnection.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout), CommonRequirementSets.PhysicsOnly)
                .Build();

            return playerTemplate;
        }
    }
}
