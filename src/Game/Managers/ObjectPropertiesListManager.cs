#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using ClassicUO.Game.GameObjects;
using ClassicUO.Renderer;
using ClassicUO.Network;

namespace ClassicUO.Game.Managers
{
    internal sealed class ObjectPropertiesListManager
    {
        private readonly Dictionary<uint, ItemProperty> _itemsProperties = new Dictionary<uint, ItemProperty>();

        public void Add(uint serial, uint revision, string name, string data)
        {
            int env_step = Client.Game._uoServiceImpl.GetEnvStep();

            if (SerialHelper.IsItem(serial))
            {
                if ((World.Player != null) && (World.InGame == true)) 
                {
                    Item item = World.Items.Get(serial);
                    try
                    {   
                        //Console.WriteLine("OPL Add() Success Item / serial: {0}, name: {1}, step: {2}", serial, name, env_step);
                        Client.Game._uoServiceImpl.AddItemObject( (uint) item.Distance, (uint) item.X, (uint) item.Y, 
                                                                  item.Serial, name, item.IsCorpse, item.Amount, item.Price, 
                                                                  (uint) item.Layer, (uint) item.Container, data );
                    }
                    catch (Exception ex) 
                    {
                        //Console.WriteLine("Failed to add the item of world: " + ex.Message);
                        //Console.WriteLine("OPL Add() Fail Item / serial: {0}, name: {1}, step: {2}", serial, name, env_step);
                    }
                }
            }
            else
            {
                if ((World.Player != null) && (World.InGame == true)) 
                {
                    Mobile mobile = World.Mobiles.Get(serial);

                    try
                    {
                        string title = "None";
                        try
                        {
                            TextObject mobileTextContainerItems = (TextObject) mobile.TextContainer.Items;
                            RenderedText renderedText = mobileTextContainerItems.RenderedText;

                            title = renderedText.Text;
                        }
                        catch (Exception ex) 
                        {
                            //Console.WriteLine("Failed to print the TextContainer Items of Mobile: " + ex.Message);
                        }

                        Client.Game._uoServiceImpl.AddMobileObject((uint) mobile.Hits, (uint) mobile.HitsMax, (uint) mobile.Race, 
                                                                   (uint) mobile.Distance, (uint) mobile.X, (uint) mobile.Y, 
                                                                   mobile.Serial, name, title, (uint) mobile.NotorietyFlag);
                    }
                    catch (Exception ex) 
                    {
                        //Console.WriteLine("Failed to add the mobile of world: " + ex.Message);
                        //Console.WriteLine("OPL Add() Mobile / serial: {0}, name: {1}, step: {2}", serial, name, env_step);
                    }

                }
            }

            if (!_itemsProperties.TryGetValue(serial, out ItemProperty prop))
            {
                prop = new ItemProperty();
                _itemsProperties[serial] = prop;
            }
            else
            {
                
            }

            prop.Serial = serial;
            prop.Revision = revision;
            prop.Name = name;
            prop.Data = data;
        }

        public bool Contains(uint serial)
        {
            if (_itemsProperties.TryGetValue(serial, out ItemProperty p))
            {
                return true; //p.Revision != 0;  <-- revision == 0 can contain the name.
            }

            // if we don't have the OPL of this item, let's request it to the server.
            // Original client seems asking for OPL when character is not running. 
            // We'll ask OPL when mouse is over an object.
            PacketHandlers.AddMegaClilocRequest(serial);

            return false;
        }

        public bool IsRevisionEquals(uint serial, uint revision)
        {
            if (_itemsProperties.TryGetValue(serial, out ItemProperty prop))
            {
                return (revision & ~0x40000000) == prop.Revision || // remove the mask
                       revision == prop.Revision;                   // if mask removing didn't work, try a simple compare.
            }

            return false;
        }

        public bool TryGetRevision(uint serial, out uint revision)
        {
            if (_itemsProperties.TryGetValue(serial, out ItemProperty p))
            {
                revision = p.Revision;

                return true;
            }

            revision = 0;

            return false;
        }

        public bool TryGetNameAndData(uint serial, out string name, out string data)
        {
            if (_itemsProperties.TryGetValue(serial, out ItemProperty p))
            {
                name = p.Name;
                data = p.Data;

                return true;
            }

            name = data = null;

            return false;
        }

        public void Remove(uint serial)
        {
            TryGetNameAndData(serial, out string name, out string data);

            int env_step = Client.Game._uoServiceImpl.GetEnvStep();
            if (name != null)
            {
                if (SerialHelper.IsItem(serial))
                {
                    //Console.WriteLine("OPL Remove() Item / serial: {0}, name: {1}, env_step: {2}", serial, name, env_step);
                    Client.Game._uoServiceImpl.AddDeleteItemSerial(serial);
                }
                else
                {
                    //Console.WriteLine("OPL Remove() Mobile / serial: {0}, name: {1}, env_step: {2}", serial, name, env_step);
                    Client.Game._uoServiceImpl.AddDeleteMobileSerial(serial);
                }
            }

            _itemsProperties.Remove(serial);
        }

        public void Clear()
        {
            _itemsProperties.Clear();
        }
    }

    internal class ItemProperty
    {
        public bool IsEmpty => string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Data);
        public string Data;
        public string Name;
        public uint Revision;
        public uint Serial;

        public string CreateData(bool extended)
        {
            return string.Empty;
        }
    }
}