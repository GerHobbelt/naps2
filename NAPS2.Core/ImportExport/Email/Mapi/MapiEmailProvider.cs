using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Mapi
{
    public class MapiEmailProvider : IEmailProvider
    {
        private readonly IErrorOutput errorOutput;
        private readonly SystemEmailClients systemEmailClients;
        private readonly IUserConfigManager userConfigManager;

        public MapiEmailProvider(IErrorOutput errorOutput, SystemEmailClients systemEmailClients, IUserConfigManager userConfigManager)
        {
            this.errorOutput = errorOutput;
            this.systemEmailClients = systemEmailClients;
            this.userConfigManager = userConfigManager;
        }

        /// <summary>
        /// Sends an email described by the given message object.
        /// </summary>
        /// <param name="message">The object describing the email message.</param>
        /// <returns>Returns true if the message was sent, false if the user aborted.</returns>
        public bool SendEmail(EmailMessage message)
        {
            // Translate files & recipients to unmanaged MAPI structures
            using (var files = Unmanaged.CopyOf(GetFiles(message)))
            using (var recips = Unmanaged.CopyOf(GetRecips(message)))
            {
                // Create a MAPI structure for the entirety of the message
                var mapiMessage = new MapiMessage
                {
                    subject = message.Subject,
                    noteText = message.BodyText,
                    recips = recips,
                    recipCount = recips.Length,
                    files = files,
                    fileCount = files.Length
                };

                // Determine the flags used to send the message
                var flags = MapiSendMailFlags.None;
                if (!message.AutoSend)
                {
                    flags |= MapiSendMailFlags.Dialog;
                }
                if (!message.AutoSend || !message.SilentSend)
                {
                    flags |= MapiSendMailFlags.LogonUI;
                }

                // Send the message
                var clientName = userConfigManager.Config.EmailSetup?.SystemProviderName;
                var mapiSendMail = systemEmailClients.GetDelegate(clientName);
                var returnCode = mapiSendMail(IntPtr.Zero, IntPtr.Zero, mapiMessage, flags, 0);

                // Process the result
                if (returnCode == MapiSendMailReturnCode.UserAbort)
                {
                    return false;
                }
                if (returnCode != MapiSendMailReturnCode.Success)
                {
                    Log.Error("Error sending email. MAPI error code: {0}", returnCode);
                    errorOutput.DisplayError(MiscResources.EmailError, $"MAPI returned error code: {returnCode}");
                    return false;
                }
                return true;
            }
        }

        private static MapiRecipDesc[] GetRecips(EmailMessage message)
        {
            return message.Recipients.Select(recipient => new MapiRecipDesc
            {
                name = recipient.Name,
                address = "SMTP:" + recipient.Address,
                recipClass = recipient.Type == EmailRecipientType.Cc ? MapiRecipClass.Cc
                    : recipient.Type == EmailRecipientType.Bcc ? MapiRecipClass.Bcc
                        : MapiRecipClass.To
            }).ToArray();
        }

        private static MapiFileDesc[] GetFiles(EmailMessage message)
        {
            return message.Attachments.Select(attachment => new MapiFileDesc
            {
                position = -1,
                path = attachment.FilePath,
                name = attachment.AttachmentName
            }).ToArray();
        }
    }
}
