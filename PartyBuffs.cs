﻿namespace CurePlease
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows.Forms;
  using System.Xml.Linq;

  public partial class PartyBuffs : Form
  {
    private readonly Form1 f1;

    public class BuffList
    {
      public string ID { get; set; }
      public string Name { get; set; }
    }

    public List<BuffList> XMLBuffList = new List<BuffList>();

    public PartyBuffs(Form1 f)
    {
      StartPosition = FormStartPosition.CenterScreen;

      InitializeComponent();

      f1 = f;

      if (f1.setinstance2.Enabled == true)
      {
        // Create the required List

        // Read the Buffs file a generate a List to call.
        foreach (var BuffElement in XElement.Load("Resources/Buffs.xml").Elements("o"))
        {
          XMLBuffList.Add(new BuffList() { ID = BuffElement.Attribute("id").Value, Name = BuffElement.Attribute("en").Value });
        }
      }
      else
      {
        MessageBox.Show("No character was selected as the power leveler, this can not be opened yet.");
      }
    }

    private void update_effects_Tick(object sender, EventArgs e)
    {
      ailment_list.Text = "";

      // Search through current active party buffs
      foreach (var ailment in f1.activeBuffs)
      {
        // First add Character name and a Line Break.
        ailment_list.AppendText(ailment.CharacterName.ToUpper() + "\n");

        // Now create a list and loop through each buff and name them
        var named_buffs = ailment.CharacterBuffs.Split(',').ToList();

        var i = 1;
        var count = named_buffs.Count();

        foreach (var acBuff in named_buffs)
        {
          i++;

          var found_Buff = XMLBuffList.Find(r => r.ID == acBuff);

          if (found_Buff != null)
          {
            if (i == count)
            {
              ailment_list.AppendText(found_Buff.Name + " (" + acBuff + ") ");
            }
            else
            {
              ailment_list.AppendText(found_Buff.Name + " (" + acBuff + "), ");
            }
          }
        }

        ailment_list.AppendText("\n\n");
      }
    }
  }
}
