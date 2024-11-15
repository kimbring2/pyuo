﻿#region license

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
using System.IO;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class ContainerGump : TextContainerGump
    {
        private long _corpseEyeTicks;
        private ContainerData _data;
        private int _eyeCorspeOffset;
        private GumpPic _eyeGumpPic;
        private GumpPicContainer _gumpPicContainer;
        private readonly bool _hideIfEmpty;
        private HitBox _hitBox;
        private bool _isMinimized;

        public ContainerGump() : base(0, 0)
        {
        }

        public ContainerGump(uint serial, ushort gumpid, bool playsound) : base(serial, 0)
        {
            Item item = World.Items.Get(serial);
            if (item == null)
            {
                Dispose();
                return;
            }

            Graphic = gumpid;

            //Console.WriteLine("ContainerGump() / serial: {0}, Graphic: {1}", serial, Graphic);
            //ContainerGump() / serial: 1073744268, Graphic: 60
            //ContainerGump() / serial: 1074023742, Graphic: 61
            //ContainerGump() / serial: 1073975432, Graphic: 9

            // New Backpack gumps. Client Version 7.0.53.1
            if (item == World.Player.FindItemByLayer(Layer.Backpack) && Client.Version >= ClassicUO.Data.ClientVersion.CV_705301 && ProfileManager.CurrentProfile != null)
            {
                GumpsLoader loader = GumpsLoader.Instance;

                //Console.WriteLine("BackpackStyle: {0}", ProfileManager.CurrentProfile.BackpackStyle);

                switch (ProfileManager.CurrentProfile.BackpackStyle)
                {
                    case 1:
                        if (loader.GetGumpTexture(0x775E, out _) != null)
                        {
                            Graphic = 0x775E; // Suede Backpack
                        }

                        break;
                    case 2:
                        if (loader.GetGumpTexture(0x7760, out _) != null)
                        {
                            Graphic = 0x7760; // Polar Bear Backpack
                        }

                        break;
                    case 3:
                        if (loader.GetGumpTexture(0x7762, out _) != null)
                        {
                            Graphic = 0x7762; // Ghoul Skin Backpack
                        }

                        break;
                    default:
                        if (loader.GetGumpTexture(0x003C, out _) != null)
                        {
                            Graphic = 0x003C; // Default Backpack
                        }

                        break;
                }
            }

            //Console.WriteLine("Graphic: {0}", Graphic);

            BuildGump();

            if (Graphic == 0x0009)
            {
                //Console.WriteLine("Graphic == 0x0009");

                if (World.Player.ManualOpenedCorpses.Contains(LocalSerial))
                {
                    //Console.WriteLine("World.Player.ManualOpenedCorpses.Contains(LocalSerial)");
                    //World.Player.ManualOpenedCorpses.Remove(LocalSerial);
                }
                else if (World.Player.AutoOpenedCorpses.Contains(LocalSerial) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.SkipEmptyCorpse)
                {
                    IsVisible = false;
                    _hideIfEmpty = true;
                }
            }

            if (_data.OpenSound != 0 && playsound)
            {
                Client.Game.Scene.Audio.PlaySound(_data.OpenSound);
            }
        }

        public ushort Graphic { get; }

        public override GumpType GumpType => GumpType.Container;

        public bool IsMinimized
        {
            get => _isMinimized;
            set
            {
                //if (_isMinimized != value)
                {
                    _isMinimized = value;
                    _gumpPicContainer.Graphic = value ? _data.IconizedGraphic : Graphic;
                    float scale = GetScale();

                    Width = _gumpPicContainer.Width = (int) (_gumpPicContainer.Width * scale);
                    Height = _gumpPicContainer.Height = (int) (_gumpPicContainer.Height * scale);

                    foreach (Control c in Children)
                    {
                        c.IsVisible = !value;
                    }

                    _gumpPicContainer.IsVisible = true;

                    SetInScreen();
                }
            }
        }

        public bool IsChessboard => Graphic == 0x091A || Graphic == 0x092E;

        private void BuildGump()
        {
            //Console.WriteLine("BuildGump()");

            CanMove = true;
            CanCloseWithRightClick = true;
            WantUpdateSize = false;

            Item item = World.Items.Get(LocalSerial);
            //Console.WriteLine("item: {0}", item);

            if (item == null)
            {
                Dispose();
                return;
            }
            else
            {
                if (item.IsCorpse) 
                {
                    //Console.WriteLine("Open corpse");
                    World.Player.ManualOpenedCorpses.Add(LocalSerial);
                }
            }

            float scale = GetScale();

            _data = ContainerManager.Get(Graphic);
            ushort g = _data.Graphic;

            _gumpPicContainer?.Dispose();
            _hitBox?.Dispose();

            _hitBox = new HitBox((int) (_data.MinimizerArea.X * scale), (int) (_data.MinimizerArea.Y * scale), (int) (_data.MinimizerArea.Width * scale), (int) (_data.MinimizerArea.Height * scale));

            _hitBox.MouseUp += HitBoxOnMouseUp;
            Add(_hitBox);

            Add(_gumpPicContainer = new GumpPicContainer(0, 0, g, 0));
            _gumpPicContainer.MouseDoubleClick += GumpPicContainerOnMouseDoubleClick;

            if (Graphic == 0x0009)
            {
                _eyeGumpPic?.Dispose();
                Add(_eyeGumpPic = new GumpPic((int) (45 * scale), (int) (30 * scale), 0x0045, 0));

                _eyeGumpPic.Width = (int) (_eyeGumpPic.Width * scale);
                _eyeGumpPic.Height = (int) (_eyeGumpPic.Height * scale);
            }

            Width = _gumpPicContainer.Width = (int) (_gumpPicContainer.Width * scale);
            Height = _gumpPicContainer.Height = (int) (_gumpPicContainer.Height * scale);

            //Console.WriteLine("Width: {0}, Height: {1}", Width, Height);
        }

        private void HitBoxOnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtonType.Left && !IsMinimized && !ItemHold.Enabled)
            {
                Point offset = Mouse.LDragOffset;
                if (Math.Abs(offset.X) < Constants.MIN_PICKUP_DRAG_DISTANCE_PIXELS && Math.Abs(offset.Y) < Constants.MIN_PICKUP_DRAG_DISTANCE_PIXELS)
                {
                    IsMinimized = true;
                }
            }
        }

        private void GumpPicContainerOnMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
        {
            if (e.Button == MouseButtonType.Left && IsMinimized)
            {
                IsMinimized = false;
                e.Result = true;
            }
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            //Console.WriteLine("ContainerGump OnMouseUp() / x: {0}, y: {1}", x, y);

            if (button != MouseButtonType.Left || UIManager.IsMouseOverWorld)
            {
                return;
            }

            Entity it = SelectedObject.Object as Entity;
            uint serial = it != null ? it.Serial : 0;
            uint dropcontainer = LocalSerial;

            if (TargetManager.IsTargeting && !ItemHold.Enabled && SerialHelper.IsValid(serial))
            {
                TargetManager.Target(serial);
                Mouse.CancelDoubleClick = true;

                if (TargetManager.TargetingState == CursorTarget.SetTargetClientSide)
                {
                    UIManager.Add(new InspectorGump(World.Get(serial)));
                }
            }
            else
            {
                Entity thisCont = World.Items.Get(dropcontainer);
                if (thisCont == null)
                {
                    return;
                }

                thisCont = World.Get(((Item) thisCont).RootContainer);
                if (thisCont == null)
                {
                    return;
                }

                bool candrop = thisCont.Distance <= Constants.DRAG_ITEMS_DISTANCE;
                if (candrop && SerialHelper.IsValid(serial))
                {
                    candrop = false;

                    if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
                    {
                        candrop = true;

                        Item target = World.Items.Get(serial);
                        if (target != null)
                        {
                            if (target.ItemData.IsContainer)
                            {
                                dropcontainer = target.Serial;
                                x = 0xFFFF;
                                y = 0xFFFF;
                            }
                            else if (target.ItemData.IsStackable && target.Graphic == ItemHold.Graphic)
                            {
                                dropcontainer = target.Serial;
                                x = target.X;
                                y = target.Y;
                            }
                            else
                            {
                                switch (target.Graphic)
                                {
                                    case 0x0EFA:
                                    case 0x2253:
                                    case 0x2252:
                                    case 0x238C:
                                    case 0x23A0:
                                    case 0x2D50:
                                    {
                                        dropcontainer = target.Serial;
                                        x = target.X;
                                        y = target.Y;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!candrop && ItemHold.Enabled && !ItemHold.IsFixedPosition)
                {
                    Client.Game.Scene.Audio.PlaySound(0x0051);
                }

                if (candrop && ItemHold.Enabled && !ItemHold.IsFixedPosition)
                {
                    //Console.WriteLine("candrop && ItemHold.Enabled && !ItemHold.IsFixedPosition");

                    ContainerGump gump = UIManager.GetGump<ContainerGump>(dropcontainer);
                    if (gump != null && (it == null || it.Serial != dropcontainer && it is Item item && !item.ItemData.IsContainer))
                    {
                        if (gump.IsChessboard)
                        {
                            y += 20;
                        }

                        Rectangle containerBounds = ContainerManager.Get(gump.Graphic).Bounds;

                        var texture = gump.IsChessboard ?
                            GumpsLoader.Instance.GetGumpTexture((ushort) (ItemHold.DisplayedGraphic - Constants.ITEM_GUMP_TEXTURE_OFFSET), out var bounds) 
                            : 
                            ArtLoader.Instance.GetStaticTexture(ItemHold.DisplayedGraphic, out bounds);

                        float scale = GetScale();

                        containerBounds.X = (int) (containerBounds.X * scale);
                        containerBounds.Y = (int) (containerBounds.Y * scale);
                        containerBounds.Width = (int) (containerBounds.Width * scale);
                        containerBounds.Height = (int) ((containerBounds.Height + (gump.IsChessboard ? 20 : 0)) * scale);

                        //Console.WriteLine("containerBounds X: {0}, Y: {1}, Width: {2}, Height: {3}\n", 
                        //             containerBounds.X, containerBounds.Y, containerBounds.Width, containerBounds.Height);

                        if (texture != null)
                        {
                            int textureW, textureH;

                            if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
                            {
                                textureW = (int) (bounds.Width * scale);
                                textureH = (int) (bounds.Height * scale);
                            }
                            else
                            {
                                textureW = bounds.Width;
                                textureH = bounds.Height;
                            }

                            if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.RelativeDragAndDropItems)
                            {
                                x += ItemHold.MouseOffset.X;
                                y += ItemHold.MouseOffset.Y;
                            }

                            x -= textureW >> 1;
                            y -= textureH >> 1;

                            if (x + textureW > containerBounds.Width)
                            {
                                x = containerBounds.Width - textureW;
                            }

                            if (y + textureH > containerBounds.Height)
                            {
                                y = containerBounds.Height - textureH;
                            }
                        }

                        if (x < containerBounds.X)
                        {
                            x = containerBounds.X;
                        }

                        if (y < containerBounds.Y)
                        {
                            y = containerBounds.Y;
                        }

                        x = (int) (x / scale);
                        y = (int) (y / scale);
                    }

                    GameActions.DropItem
                    (
                        ItemHold.Serial,
                        x,
                        y,
                        0,
                        dropcontainer
                    );

                    Mouse.CancelDoubleClick = true;
                }
                else if (!ItemHold.Enabled && SerialHelper.IsValid(serial))
                {
                    if (!DelayedObjectClickManager.IsEnabled)
                    {
                        Point off = Mouse.LDragOffset;

                        DelayedObjectClickManager.Set(serial, Mouse.Position.X - off.X - ScreenCoordinateX, Mouse.Position.Y - off.Y - ScreenCoordinateY, Time.Ticks + Mouse.MOUSE_DELAY_DOUBLE_CLICK);
                    }
                }
            }
        }

        public override void Update(double totalTime, double frameTime)
        {
            base.Update(totalTime, frameTime);

            if (IsDisposed)
            {
                return;
            }

            Item item = World.Items.Get(LocalSerial);
            if (item == null || item.IsDestroyed)
            {
                Dispose();

                return;
            }

            if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.RootParent == this && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.HighlightContainerWhenSelected)
            {
                SelectedObject.SelectedContainer = item;
            }

            if (Graphic == 0x0009 && _corpseEyeTicks < totalTime)
            {
                _eyeCorspeOffset = _eyeCorspeOffset == 0 ? 1 : 0;
                _corpseEyeTicks = (long) totalTime + 750;
                _eyeGumpPic.Graphic = (ushort) (0x0045 + _eyeCorspeOffset);
                float scale = GetScale();
                _eyeGumpPic.Width = (int) (_eyeGumpPic.Width * scale);
                _eyeGumpPic.Height = (int) (_eyeGumpPic.Height * scale);
            }
        }

        protected override void UpdateContents()
        {
            //Console.WriteLine("UpdateContents()");
            
            Clear();
            BuildGump();
            IsMinimized = IsMinimized;
            ItemsOnAdded();
        }

        public override void Save(XmlTextWriter writer)
        {
            base.Save(writer);
            writer.WriteAttributeString("graphic", Graphic.ToString());
            writer.WriteAttributeString("isminimized", IsMinimized.ToString());
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);
            // skip loading

            Client.Game.GetScene<GameScene>()?.DoubleClickDelayed(LocalSerial);

            Dispose();
        }

        private float GetScale()
        {
            return IsChessboard ? 1f : UIManager.ContainerScale;
        }

        private void ItemsOnAdded()
        {
            //Console.WriteLine("ContainerGump ItemsOnAdded()");

            Entity container = World.Get(LocalSerial);
            if (container == null)
            {
                return;
            }

            bool is_corpse = container.Graphic == 0x2006;
            if (!container.IsEmpty && _hideIfEmpty && !IsVisible)
            {
                IsVisible = true;
            }

            for (LinkedObject i = container.Items; i != null; i = i.Next)
            {
                Item item = (Item) i;

                if (((item.Layer == 0 && 
                    (Layer)item.ItemData.Layer != Layer.Face &&
                    (Layer)item.ItemData.Layer != Layer.Beard &&
                    (Layer)item.ItemData.Layer != Layer.Hair) || 
                    (is_corpse && Constants.BAD_CONTAINER_LAYERS[(int) item.Layer])) 
                    && item.Amount > 0)
                {
                    ItemGump itemControl = new ItemGump
                    (
                        item.Serial,
                        (ushort) (item.DisplayedGraphic - (IsChessboard ? Constants.ITEM_GUMP_TEXTURE_OFFSET : 0)),
                        item.Hue,
                        item.X,
                        item.Y,
                        IsChessboard
                    );

                    itemControl.IsVisible = !IsMinimized;

                    float scale = GetScale();

                    if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
                    {
                        itemControl.Width = (int) (itemControl.Width * scale);
                        itemControl.Height = (int) (itemControl.Height * scale);
                    }

                    itemControl.X = (int) ((short) item.X * scale);
                    itemControl.Y = (int) (((short) item.Y - (IsChessboard ? 20 : 0)) * scale);

                    Add(itemControl);
                }
            }
        }

        public void CheckItemControlPosition(Item item)
        {
            Rectangle dataBounds = _data.Bounds;

            int boundX = dataBounds.X;
            int boundY = dataBounds.Y;
            int boundWidth = dataBounds.Width;
            int boundHeight = dataBounds.Height + (IsChessboard ? 20 : 0);

            var texture = IsChessboard ? 
                GumpsLoader.Instance.GetGumpTexture((ushort) (item.DisplayedGraphic - (IsChessboard ? Constants.ITEM_GUMP_TEXTURE_OFFSET : 0)), out var bounds) 
                :
                ArtLoader.Instance.GetStaticTexture(item.DisplayedGraphic, out bounds);

            if (texture != null)
            {
                float scale = GetScale();

                boundWidth -= (int) (bounds.Width / scale);
                boundHeight -= (int) (bounds.Height / scale);
            }

            if (item.X < boundX)
            {
                item.X = (ushort) boundX;
            }
            else if (item.X > boundWidth)
            {
                item.X = (ushort) boundWidth;
            }

            if (item.Y < boundY)
            {
                item.Y = (ushort) boundY;
            }
            else if (item.Y > boundHeight)
            {
                item.Y = (ushort) boundHeight;
            }
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            base.Draw(batcher, x, y);

            if (CUOEnviroment.Debug && !IsMinimized)
            {
                Rectangle bounds = _data.Bounds;
                float scale = GetScale();
                ushort boundX = (ushort) (bounds.X * scale);
                ushort boundY = (ushort) (bounds.Y * scale);
                ushort boundWidth = (ushort) (bounds.Width * scale);
                ushort boundHeight = (ushort) (bounds.Height * scale);

                Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);

                batcher.DrawRectangle
                (
                    SolidColorTextureCache.GetTexture(Color.Red),
                    x + boundX,
                    y + boundY,
                    boundWidth - boundX,
                    boundHeight - boundY,
                    hueVector
                );
            }

            return true;
        }

        public override void Dispose()
        {
            //Console.WriteLine("Dispose()");

            try
            {
                Item item = World.Items.Get(LocalSerial);

                if (item.IsCorpse) 
                {
                    World.Player.ManualOpenedCorpses.Remove(LocalSerial);
                }

                if (item != null)
                {
                    if (World.Player != null && ProfileManager.CurrentProfile?.OverrideContainerLocationSetting == 3)
                    {
                        UIManager.SavePosition(item, Location);
                    }

                    for (LinkedObject i = item.Items; i != null; i = i.Next)
                    {
                        Item child = (Item) i;
                        if (child.Container == item)
                        {
                            UIManager.GetGump<ContainerGump>(child)?.Dispose();
                        }
                    }
                }

                base.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to dispose the corpse container: " + ex.Message);
            }
        }

        private static void ClearContainerAndRemoveItems(Entity container, bool remove_unequipped = false)
        {
            if (container == null || container.IsEmpty)
            {
                return;
            }

            //Console.WriteLine("ClearContainerAndRemoveItems()");

            LinkedObject first = container.Items;
            LinkedObject new_first = null;

            while (first != null)
            {
                LinkedObject next = first.Next;
                Item it = (Item) first;

                if (remove_unequipped && it.Layer != 0)
                {
                    if (new_first == null)
                    {
                        new_first = first;
                    }
                }
                else
                {
                    World.OPL.TryGetNameAndData(it.Serial, out string name, out string data);
                    //Console.WriteLine("World.RemoveItem(it, true): {0}", name);

                    World.RemoveItem(it, true);
                }

                first = next;
            }

            container.Items = remove_unequipped ? new_first : null;

            //Client.Game._uoServiceImpl.UpdateWorldItems();
        }

        protected override void CloseWithRightClick()
        {
            //Console.WriteLine("CloseWithRightClick()");

            Item item = World.Items.Get(LocalSerial);
            if (item.IsCorpse) 
            {
                //Console.WriteLine("Clsoe corpse");
                World.Player.ManualOpenedCorpses.Remove(LocalSerial);
            }

            base.CloseWithRightClick();

            if (_data.ClosedSound != 0)
            {
                Client.Game.Scene.Audio.PlaySound(_data.ClosedSound);
            }

            if (item != null)
            {
                //Console.WriteLine("it != null");

                item.Opened = false;
                if (!item.IsCorpse)
                {
                    ClearContainerAndRemoveItems(item);
                }
            }

            World.OPL.TryGetNameAndData(item.Serial, out string name, out string data);
            if (name != null) 
            {
                //Console.WriteLine("UpdateGameObject() item, name: {0}", name);
                try
                {   
                    //Console.WriteLine("OPL Add() Success Item / serial: {0}, name: {1}, step: {2}", serial, name, env_step);
                    Client.Game._uoServiceImpl.AddItemObject( (uint) item.Distance, (uint) item.X, (uint) item.Y, 
                                                              item.Serial, name, item.IsCorpse, item.Amount, item.Price, 
                                                              (uint) item.Layer, (uint) item.Container, data, (bool) item.Opened);
                }
                catch (Exception ex) 
                {
                    //Console.WriteLine("Failed to add the item of world: " + ex.Message);
                    //Console.WriteLine("OPL Add() Fail Item / serial: {0}, name: {1}, step: {2}", serial, name, env_step);
                }
            }
        }

        public void CloseWindow()
        {
            //Console.WriteLine("CloseWindow()");
            CloseWithRightClick();
        }

        protected override void OnDragEnd(int x, int y)
        {
            if (ProfileManager.CurrentProfile.OverrideContainerLocation && ProfileManager.CurrentProfile.OverrideContainerLocationSetting >= 2)
            {
                Point gumpCenter = new Point(X + (Width >> 1), Y + (Height >> 1));
                ProfileManager.CurrentProfile.OverrideContainerLocationPosition = gumpCenter;
            }

            base.OnDragEnd(x, y);
        }

        private class GumpPicContainer : GumpPic
        {
            public GumpPicContainer(int x, int y, ushort graphic, ushort hue) : base(x, y, graphic, hue)
            {
            }

            public override bool Contains(int x, int y)
            {
                float scale = Graphic == 0x091A || Graphic == 0x092E ? 1f : UIManager.ContainerScale;

                x = (int) (x / scale);
                y = (int) (y / scale);

                return base.Contains(x, y);
            }
        }
    }
}