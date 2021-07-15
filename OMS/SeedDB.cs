using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OMS.Data;
using OMS.Models;

namespace OMS.Dev
{
    public static class DevDB
    {
        public static async Task SeedDB(IHost host)
        {
            Member kurt = new Member()
            {
                Title = "Mr.",
                FirstName = "Kurt",
                LastName = "Carré",
                Email = "kurtcarre569@gmail.com",
                Address1 = "Les Arbres",
                Address2 = "Les Gigands",
                Parish = "St. Sampson",
                PostCode = "GY2 4YX",
                MemberType = "Playing",
                Under18 = false,
                Section = "Brass",
                Instrument = "French Horn",
                DateJoined = new DateTime(2020, 11, 20)
            };

            Member fred = new Member()
            {
                Title = "Mr.",
                FirstName = "Fred",
                LastName = "Flintstone",
                Email = "fred@flintstone.com",
                Address1 = "Cave",
                Address2 = "Cave Street",
                Parish = "Bedrock",
                PostCode = "BB2 4GY",
                MemberType = "Playing",
                Under18 = false,
                Section = "Misc.",
                Instrument = "Percussion",
                DateJoined = new DateTime(2021, 2, 10)
            };

            Member billy = new Member()
            {
                Title = "Mr.",
                FirstName = "Billy",
                LastName = "Bob",
                Email = "billy.mum@bob.com",
                Address1 = "Music House",
                Address2 = "Rhythm Road",
                Parish = "Symphony",
                PostCode = "OP01 9KV",
                MemberType = "Playing",
                Under18 = true,
                Section = "Strings",
                Instrument = "Violin",
                DateJoined = new DateTime(2020, 12, 11)
            };

            ChildMember billyChild = new ChildMember()
            {
                Member = billy,
                ParentFirstName = "Jill",
                ParentLastName = "Bob",
                EmergencyContactNo = "07781 111234",
                DoB = new DateTime(2009, 5, 15),
                Consent = true
            };

            Member lily = new Member()
            {
                Title = "Miss",
                FirstName = "Lily",
                LastName = "Potter",
                Email = "lily.potter@evans.hp",
                Address1 = "No 10 Grimauld Place",
                Address2 = "Godric's Hollow",
                Parish = "London",
                PostCode = "LL14 4FF",
                MemberType = "Playing",
                Section = "Woodwind",
                Instrument = "Clarinet",
                Under18 = true,
                DateJoined = new DateTime(2021, 2, 7)
            };

            ChildMember lilyChild = new ChildMember()
            {
                Member = lily,
                ParentFirstName = "Harry",
                ParentLastName = "Potter",
                EmergencyContactNo = "07781 463821",
                DoB = new DateTime(2007, 5, 30),
                Consent = true
            };

            Member albus = new Member()
            {
                Title = "Mr.",
                FirstName = "Albus",
                LastName = "Potter",
                Email = "albus@hogwarts.com",
                Address1 = "Hogwarts",
                Address2 = "Hogwarts Road",
                Parish = "Hogsmeade",
                PostCode = "HP11 9HW",
                MemberType = "Playing",
                Section = "Strings",
                Instrument = "Cello",
                Under18 = true,
                DateJoined = new DateTime(2021, 3, 2)
            };

            ChildMember albusChild = new ChildMember()
            {
                Member = albus,
                ParentFirstName = "Harry",
                ParentLastName = "Potter",
                EmergencyContactNo = "07781 463821",
                DoB = new DateTime(2008, 5, 4),
                Consent = true
            };

            Member rose = new Member()
            {
                Title = "Miss",
                FirstName = "Rose",
                LastName = "Weasley",
                Email = "rose.weasley@mom.gov.uk",
                Address1 = "The Burrow",
                Address2 = "",
                Parish = "Little Whinning",
                PostCode = "BU45 2GG",
                MemberType = "Playing",
                Section = "Brass",
                Instrument = "Trumpet",
                Under18 = true,
                DateJoined = new DateTime(2021, 2, 9)
            };

            ChildMember roseChild = new ChildMember()
            {
                Member = rose,
                ParentFirstName = "Hermione",
                ParentLastName = "Granger",
                EmergencyContactNo = "07781 458924",
                DoB = new DateTime(2005, 2, 1),
                Consent = true
            };

            Member barney = new Member()
            {
                Title = "Mr.",
                FirstName = "Barney",
                LastName = "Rubble",
                Email = "barney@bedrockisp.com",
                Address1 = "Next to Fred's",
                Address2 = "Cave Road",
                Parish = "Bedrock",
                PostCode = "BB2 4GY",
                MemberType = "Playing",
                Section = "Brass",
                Instrument = "Trombone",
                Under18 = false,
                DateJoined = new DateTime(2021, 2, 11)
            };

            Member bambam = new Member()
            {
                Title = "Miss",
                FirstName = "BamBam",
                LastName = "Flintstone",
                Email = "fred@flintstone.com",
                Address1 = "Cave",
                Address2 = "Cave Road",
                Parish = "Bedrock",
                PostCode = "BB2 4GY",
                MemberType = "Playing",
                Section = "Woodwind",
                Instrument = "Oboe",
                Under18 = true,
                DateJoined = new DateTime(2021, 2, 10)
            };

            ChildMember bambamChild = new ChildMember()
            {
                Member = bambam,
                ParentFirstName = "Fred",
                ParentLastName = "Flintstone",
                EmergencyContactNo = "07781 745845",
                DoB = new DateTime(2004, 2, 11),
                Consent = true
            };

            var services = host.Services.CreateScope().ServiceProvider;
            var context = services.GetRequiredService<DBContext>();

            context.Add(kurt);
            context.Add(fred);
            context.Add(billy);
            context.Add(billyChild);
            context.Add(lily);
            context.Add(lilyChild);
            context.Add(albus);
            context.Add(albusChild);
            context.Add(rose);
            context.Add(roseChild);
            context.Add(barney);
            context.Add(bambam);
            context.Add(bambamChild);

            await context.SaveChangesAsync();
        }
    }
}