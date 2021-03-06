using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Extensions;
using EVEMon.Common.Service;
using YamlDotNet.RepresentationModel;

namespace EVEMon.Common.Models.Extended
{
    /// <summary>
    /// A developing class for the external parser of the EVE notification text.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be called from anywhere inside the application.
    /// Its only purpose for being included in the codebase is for formatting and compiling reasons.
    /// </remarks>
    public sealed class ExternalEveNotificationTextParser : EveNotificationTextParser
    {
        public override void Parse(EveNotification notification, KeyValuePair<YamlNode, YamlNode> pair,
            IDictionary<string, string> parsedDict)
        {
            switch (pair.Key.ToString().ToUpperInvariant())
            {
                case "CHARID":
                case "SENDERCHARID":
                case "RECEIVERCHARID":
                case "OWNERID":
                case "LOCATIONOWNERID":
                case "DESTROYERID":
                case "INVOKINGCHARID":
                case "CORPID":
                case "PODKILLERID":
                case "NEWCEOID":
                case "OLDCEOID":
                {
                    parsedDict[pair.Key.ToString()] = EveIDToName.GetIDToName(pair.Value.ToString());
                    break;
                }
                case "CLONESTATIONID":
                case "CORPSTATIONID":
                case "LOCATIONID":
                {
                    parsedDict[pair.Key.ToString()] = Station.GetByID(int.Parse(pair.Value.ToString())).Name;
                    break;
                }
                case "SHIPTYPEID":
                case "TYPEID":
                {
                    parsedDict[pair.Key.ToString()] = StaticItems.GetItemByID(int.Parse(pair.Value.ToString())).Name;
                    break;
                }
                case "MEDALID":
                {
                    var medal = notification.CCPCharacter.CharacterMedals
                        .FirstOrDefault(x => x.ID.ToString() == pair.Value.ToString());

                    parsedDict[pair.Key.ToString()] = medal == null
                        ? EveMonConstants.UnknownText
                        : medal.Title ?? EveMonConstants.UnknownText;

                    parsedDict.Add("medalDescription", medal == null
                        ? EveMonConstants.UnknownText
                        : medal.Description ?? EveMonConstants.UnknownText);
                    break;
                }
                case "ENDDATE":
                case "STARTDATE":
                {
                    parsedDict[pair.Key.ToString()] = string.Format(CultureConstants.InvariantCulture,
                        "{0:dddd, MMMM d, yyyy HH:mm} (EVE Time)", long.Parse(pair.Value.ToString())
                            .WinTimeStampToDateTime());
                    break;
                }
                case "NOTIFICATION_CREATED":
                {
                    parsedDict[pair.Key.ToString()] = string.Format(CultureConstants.InvariantCulture,
                        "{0:dddd, MMMM d, yyyy} (EVE Time)", long.Parse(pair.Value.ToString())
                            .WinTimeStampToDateTime());
                    break;
                }
                case "TYPEIDS":
                {
                    YamlSequenceNode typeIDs = pair.Value as YamlSequenceNode;

                    if (typeIDs == null)
                        break;

                    switch (notification.TypeID)
                    {
                        case 56:
                        case 57:
                        {
                            if (!typeIDs.Any())
                                parsedDict[pair.Key.ToString()] = "None were in the clone";
                            else
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (var typeID in typeIDs)
                                {
                                    sb
                                        .AppendLine()
                                        .AppendLine($"Type: {StaticItems.GetItemByID(int.Parse(typeID.ToString())).Name}");
                                }
                                parsedDict[pair.Key.ToString()] = sb.ToString();
                            }
                        }
                            break;
                    }
                    break;
                }
                case "ISHOUSEWARMINGGIFT":
                {
                    if (!Convert.ToBoolean(pair.Value))
                        break;

                    switch (notification.TypeID)
                    {
                        case 34:
                            // Tritanium
                            parsedDict[pair.Key.ToString()] = StaticItems.GetItemByID(34).Name;
                            break;
                    }
                    break;
                }
                case "LEVEL":
                {
                    parsedDict[pair.Key.ToString()] = $"{Standing.Status(double.Parse(pair.Value.ToString()))} Standing";
                    break;
                }
            }
        }
    }
}
