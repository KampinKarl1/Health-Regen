using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCameraComponents
{
    /// <summary>
    /// Using this to create fine/smooth movements with Scoped aiming.
    /// </summary>
    [System.Serializable]
    public class SniperMouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        float[] lastY = { 0f, 0f };
        float[] lastX = { 0f, 0f };

        float yRot = 0f;
        float xRot = 0f;

        const float THRES = .8f;

        /// <summary>
        /// Pass in the character Transform reference so its rotation will be changed toward the target rotation.
        /// Same for the camera's Transform refernce. Think of this as LookRotation (Transform &character, Transform &camera) [but you already know classes are passed by ref in C# you pro]
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        public void LookRotation(Transform character, Transform camera)
        {
            //Get last two frames' rotation values
            lastY[1] = lastY[0];
            lastX[1] = lastX[0];

            lastY[0] = yRot;
            lastX[0] = xRot;

            yRot = Input.GetAxis("Mouse X") * XSensitivity; //CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            xRot = Input.GetAxis("Mouse Y") * YSensitivity;  //CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            //-----------------------------------------------------------------
            //If the last two rotation values are low enough, square rotation (this assumes it's less that 1.0) to make a 'smoother' rotation
            //This makes for really fine movements when sniping.
            float sign = Mathf.Sign(xRot);
            if (lastX[0] <= THRES && Mathf.Abs(xRot) < THRES)
                xRot *= xRot * sign;

            sign = Mathf.Sign(yRot);
            if (lastY[0] <= THRES && Mathf.Abs(yRot) < THRES)
                yRot *= yRot * sign;
            //-----------------------------------------------------------------


            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}