﻿/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware {

  internal delegate void ControlEventHandler(Control control);

  internal class Control : IControl {

    private readonly Identifier identifier;
    private readonly ISettings settings;
    private ControlMode mode;
    private FanMode fanMode;
    private FanFollow fanFollow;
    private float softwareValue;
    private float minSoftwareValue;
    private float maxSoftwareValue;
      //fan add. only for init of fan duty value(50)
    private SensorType sensorType;

    public Control(ISensor sensor, ISettings settings, float minSoftwareValue,
      float maxSoftwareValue) 
    {
      this.identifier = new Identifier(sensor.Identifier, "control");
      this.settings = settings;
      this.minSoftwareValue = minSoftwareValue;
      this.maxSoftwareValue = maxSoftwareValue;
      //fan add
      this.sensorType = sensor.SensorType;

      float softValue = 0;
      if (!float.TryParse(settings.GetValue(
          new Identifier(identifier, "value").ToString(), "-a"),
        NumberStyles.Float, CultureInfo.InvariantCulture,
        out softValue)){
          this.softwareValue = 0;
          if (this.sensorType.Equals(SensorType.TinyFanControl))
              this.SoftwareValue = 50; //init value(when .config was not exist)
      }
      else
          this.softwareValue = softValue;
      int mode;
      if (!int.TryParse(settings.GetValue(
          new Identifier(identifier, "mode").ToString(),
          ((int)ControlMode.Default).ToString(CultureInfo.InvariantCulture)),
        NumberStyles.Integer, CultureInfo.InvariantCulture,
        out mode)) 
      {
        this.mode = ControlMode.Default;
      } else {
        this.mode = (ControlMode)mode;
      }
      int fanMode;
      if (!int.TryParse(settings.GetValue(
          new Identifier(identifier, "fanMode").ToString(),
          ((int)FanMode.Pin4).ToString(CultureInfo.InvariantCulture)),
        NumberStyles.Integer, CultureInfo.InvariantCulture,
        out fanMode))
      {
          this.fanMode = FanMode.Pin4;
      }
      else
      {
          this.fanMode = (FanMode)fanMode;
      }
      int fanFollow;//again
      if (!int.TryParse(settings.GetValue(
          new Identifier(identifier, "fanFollow").ToString(),
          ((int)FanFollow.NONE).ToString(CultureInfo.InvariantCulture)),
        NumberStyles.Integer, CultureInfo.InvariantCulture,
        out fanFollow))
      {
          this.fanFollow = FanFollow.NONE;
      }
      else
      {
          this.fanFollow = (FanFollow)fanFollow;
      }
    }

    public Identifier Identifier {
      get {
        return identifier;
      }
    }

    public ControlMode ControlMode {
      get {
        return mode;
      }
      private set {
        if (mode != value) {
          mode = value;
          if (ControlModeChanged != null)
            ControlModeChanged(this);
          this.settings.SetValue(new Identifier(identifier, "mode").ToString(),
            ((int)mode).ToString(CultureInfo.InvariantCulture));
        }
      }
    }

    public FanMode FanMode
    {
        get
        {
            return fanMode;
        }
        private set
        {
            if (fanMode != value)
            {
                fanMode = value;
                if (FanModeChanged != null)
                    FanModeChanged(this);
                this.settings.SetValue(new Identifier(identifier, "fanMode").ToString(),
                  ((int)fanMode).ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public FanFollow FanFollow
    {
        get
        {
            return fanFollow;
        }
        private set
        {
            if (fanFollow != value)
            {
                fanFollow = value;
                if (FanFollowChanged != null)
                    FanFollowChanged(this);
                this.settings.SetValue(new Identifier(identifier, "fanFollow").ToString(),
                  ((int)fanFollow).ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public float SoftwareValue {
      get {
        return softwareValue;
      }
      private set {
        if (softwareValue != value) {
          softwareValue = value;
          if (SoftwareControlValueChanged != null)
            SoftwareControlValueChanged(this);
          this.settings.SetValue(new Identifier(identifier,
            "value").ToString(),
            value.ToString(CultureInfo.InvariantCulture));
        }
      }
    }

    public void SetDefault() {
      ControlMode = ControlMode.Default;
    }

    public float MinSoftwareValue {
      get {
        return minSoftwareValue;
      }
    }

    public float MaxSoftwareValue {
      get {
        return maxSoftwareValue;
      }
    }

    public void SetSoftware(float value) {
      ControlMode = ControlMode.Software;
      SoftwareValue = value;
    }
    public void SetTheFanMode(FanMode fanMode)
    {
        FanMode = fanMode;
    }
    public void SetTheFanFollow(FanFollow ff)
    {
        FanFollow = ff;
    }

    internal event ControlEventHandler ControlModeChanged;
    internal event ControlEventHandler FanModeChanged;
    internal event ControlEventHandler FanFollowChanged;
    internal event ControlEventHandler SoftwareControlValueChanged;
  }
}
