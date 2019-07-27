using UnityEngine;

namespace Assets.Scripts.Submarine
{
    public class CamFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 followOffset;
        public float lookAheadDst = 10;
        public float smoothTime = .1f;
        public float rotSmoothSpeed = 3;

        private Vector3 _smoothV;

    
        void LateUpdate()
        {
            Vector3 targetPos = target.position + target.forward * followOffset.z + target.up * followOffset.y + target.right * followOffset.x;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _smoothV, smoothTime);

            Quaternion rot = transform.rotation;
            transform.LookAt(target.position + target.forward * lookAheadDst);
            Quaternion targetRot = transform.rotation;

            transform.rotation = Quaternion.Slerp(rot,targetRot,Time.deltaTime * rotSmoothSpeed);
        }
    }
}
