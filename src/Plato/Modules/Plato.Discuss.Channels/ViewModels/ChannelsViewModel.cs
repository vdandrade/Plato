﻿using System.Collections.Generic;
using Plato.Categories.Models;

namespace Plato.Discuss.Channels.ViewModels
{
    public class ChannelsViewModel
    {

        public IEnumerable<Category> Channels;

        public Category EditChannel { get; set; }

        public DefaultIcons ChannelIcons { get; set; }

    }
}