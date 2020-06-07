/*
 * CapSense Click board driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision
 * 
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;

namespace MBN.Modules
{
    public sealed partial class CapSenseClick
    {
        internal static class Registers
        {
            public const Byte OUTPUT_PORT0 = 0x04;
            public const Byte CS_ENABL0 = 0x06;
            public const Byte CS_ENABLE = 0x07;
            public const Byte GPIO_ENABLE0 = 0x08;
            public const Byte DM_STRONG0 = 0x11;
            public const Byte CS_SLID_CONFIG = 0x75;
            public const Byte CS_SLID_MULM = 0x77;
            public const Byte CS_SLID_MULL = 0x78;
            public const Byte CS_READ_STATUSM = 0x88;
            public const Byte CS_READ_CEN_POSM = 0x8A;
            public const Byte COMMAND = 0xA0;
        }
    }
}
