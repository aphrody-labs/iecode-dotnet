using IESEViewModel.InazumaEleven.Logic.Competition_Route;

namespace IESEViewModel.InazumaEleven.Common.GO;

// Seems not to be used
public static class Routes
{
    public static List<Route> Cs =
    [
            new("The Starter Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Inferno Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),

                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),

                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),

                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Brawl Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Showdown Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Truth Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Revolution Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Old-School Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Hidden Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Lost Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Top-Secret Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Ultimate Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ]),


                new("The Grandfather Route", [
                        new Cell("Raimon Reserves", 8,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("All Saints' Allstars", 11,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega", 15,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Ivy Selective", 9,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Warlike Lasses", 12,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("League of Villains", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("20,000 Leagues", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("The Goalie Spirits", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 2.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Footballbots", 16,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("Group Schiller", 19,
                                [0x0B0982E2, 0xB64DE176, 0x3B0DA6EE, 0x899E6282, 0xDD9C3FD6, 0x27FA62B4]),


                        new Cell("Owari Allstars", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("White Deer", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ]),


                        new Cell("The Tri-Hards", 22,
                                [0x7EF9637B, 0x64F1E12A, 0x59762C7D, 0x4BAC38A1, 0x0B9B6C5A, 0xCF462B0A]),


                        new Cell("Protocol Omega 3.0", 25,
                        [
                                0xCBDE3E3D, 0x4B2E3F89, 0xBA4B626D, 0xE5AF007D, 0xA88F1FC4, 0x6E1C0141
                        ])
                ])
    ];
}