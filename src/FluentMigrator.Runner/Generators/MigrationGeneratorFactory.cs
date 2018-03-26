﻿#region License
//
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Generators
{
    public class MigrationGeneratorFactory
    {
        private static readonly IDictionary<string, IMigrationGenerator> MigrationGenerators;

        static MigrationGeneratorFactory()
        {
            Assembly assembly = typeof(IMigrationProcessorFactory).Assembly;

            List<Type> types = assembly
                               .GetExportedTypes()
                               .Where(type => type.IsConcrete() && type.Is<IMigrationGenerator>())
                               .ToList();

            var available = new SortedDictionary<string, IMigrationGenerator>();
            foreach (Type type in types)
            {
                try
                {
                    var factory = (IMigrationGenerator) Activator.CreateInstance(type);
                    available.Add(type.Name.Replace("Generator", ""), factory);
                }
                catch (Exception)
                {
                    //can't add generators that require construtor parameters
                }
            }

            MigrationGenerators = available;
        }

        public virtual IMigrationGenerator GetGenerator(string name)
        {
            return MigrationGenerators
                   .Where(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                   .Select(pair => pair.Value)
                   .FirstOrDefault();
        }

        public string ListAvailableProcessorTypes()
        {
            return string.Join(", ", MigrationGenerators.Keys.ToArray());
        }
    }
}
