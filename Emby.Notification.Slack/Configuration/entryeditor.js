define(['globalize', 'pluginManager', 'emby-input'], function (globalize, pluginManager) {
    'use strict';

    function EntryEditor() {
    }

    EntryEditor.setObjectValues = function (context, entry) {

        entry.FriendlyName = context.querySelector('.txtFriendlyName').value;
        entry.Options.SlackWebHookURI = context.querySelector('.txtSlackWebHookUri').value;
        entry.Options.Channel = context.querySelector('.txtSlackChannel').value;
        entry.Options.Emoji = context.querySelector('.txtSlackEmoji').value;
        entry.Options.UserName = context.querySelector('.txtSlackUserName').value;
    };

    EntryEditor.setFormValues = function (context, entry) {

        context.querySelector('.txtFriendlyName').value = entry.FriendlyName || '';
        context.querySelector('.txtSlackWebHookUri').value = entry.Options.SlackWebHookURI || '';
        context.querySelector('.txtSlackChannel').value = entry.Options.Channel || '';
        context.querySelector('.txtSlackEmoji').value = entry.Options.Emoji || '';
        context.querySelector('.txtSlackUserName').value = entry.Options.UserName || '';
    };

    EntryEditor.loadTemplate = function (context) {

        return require(['text!' + pluginManager.getConfigurationResourceUrl('slackeditortemplate')]).then(function (responses) {

            var template = responses[0];
            context.innerHTML = globalize.translateDocument(template);

            // setup any required event handlers here
        });
    };

    EntryEditor.destroy = function () {

    };

    return EntryEditor;
});