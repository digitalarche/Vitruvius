﻿//
// Copyright (c) LightBuzz Software.
// All rights reserved.
//
// http://lightbuzz.com
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
// AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

using System;

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Represents the predefined gesture types.
    /// </summary>
    public enum GestureType
    {
        /// <summary>
        /// Hands joined in front of chest.
        /// </summary>
        JoinedHands,

        /// <summary>
        /// Waving using the right hand.
        /// </summary>
        WaveRight,

        /// <summary>
        /// Waving using the left hand.
        /// </summary>
        WaveLeft,

        /// <summary>
        /// Hand slightly bent above hip (XBOX-like gesture).
        /// </summary>
        Menu,

        /// <summary>
        /// Hand moved horizontally from right to left.
        /// </summary>
        SwipeLeft,

        /// <summary>
        /// Hand moved horizontally from left to right.
        /// </summary>
        SwipeRight,

        /// <summary>
        /// Hand moved vertically from hip center to head.
        /// </summary>
        SwipeUp,

        /// <summary>
        /// Hand moved vertically from head to hip center.
        /// </summary>
        SwipeDown,

        /// <summary>
        /// Both hands extended closer to the chest.
        /// </summary>
        ZoomIn,

        /// <summary>
        /// Both hands extended farther from the chest.
        /// </summary>
        ZoomOut
    }
}
