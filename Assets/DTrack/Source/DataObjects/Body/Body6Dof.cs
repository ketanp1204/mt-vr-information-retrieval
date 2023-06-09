﻿/* Copyright (c) 2019, Advanced Realtime Tracking GmbH
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using DTrack.DataObjects.Interfaces;
using DTrack.Util;
using UnityEngine;

namespace DTrack.DataObjects.Body
{
    public class Body6Dof : Body, IVectorRotationProvider,IQuaternionRotationProvider
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public float Confidence { get; }

        public Position3 Position;
        public Rotation3x3 RotationMatrix;

        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _rotationEuler;

        private bool _positionGenerated;
        private bool _rotationGenerated;

        public Body6Dof(int id, float confidence,
                float p0, float p1, float p2,
                float m0, float m1, float m2,
                float m3, float m4, float m5,
                float m6, float m7, float m8) : base(id)
        {
            this.Confidence = confidence;

            this.Position = new Position3( p0, p1, p2 );
            this.RotationMatrix = new Rotation3x3( m0, m1, m2,  m3, m4, m5,  m6, m7, m8 );
        }

        public Vector3 GetPosition()
        {
            if (!_positionGenerated)
            {
                _position = this.Position.ToUnityPosition();
                _positionGenerated = true;
            }

            return _position;
        }

        public Quaternion GetRotation()
        {
            if (!_rotationGenerated)
            {
                _rotation = this.RotationMatrix.ToUnityQuaternion();
                _rotationGenerated = true;
            }

            return _rotation;
        }
    }
}
