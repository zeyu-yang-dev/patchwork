using System.Collections.Generic;

namespace Patchwork.Data;

/// <summary>
/// Stores id, shape, money cost, time cost and income of each patch.
/// </summary>
public static class PatchData
{
    public static readonly Dictionary<int , (bool[,] shape, int moneyCost, int timeCost, int income)> Data = new()
    {
        {
            // special patch:
            -1,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                0,
                0,
                0
            )
        },
        {
            0,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                2,
                1,
                0
            )
        },
        {
            1,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true, false, false, false },
                    { false,  true,  true,  true,  true },
                    { false,  true, false, false, false },
                    { false, false, false, false, false }
                },
                7,
                2,
                2
            )
        },
        {
            2,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    {  true,  true,  true,  true,  true },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                1,
                4,
                1
            )
        },
        {
            3,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    { false,  true,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                2,
                2,
                0
            )
        },
        {
            4,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    {  true,  true,  true,  true, false },
                    { false,  true,  true, false, false },
                    { false, false, false, false, false }
                },
                5,
                3,
                1
            )
        },
        {
            5,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false,  true,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                2,
                2,
                0
            )
        },
        {
            6,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true, false,  true, false },
                    { false,  true,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                1,
                2,
                0
            )
        },
        {
            7,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    { false, false,  true,  true, false },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                1,
                3,
                0
            )
        },
        {
            8,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true,  true, false },
                    { false, false,  true, false, false },
                    { false,  true,  true,  true, false },
                    { false, false, false, false, false }
                },
                2,
                3,
                0
            )
        },
        {
            9,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    {  true,  true,  true,  true, false },
                    { false,  true, false, false, false },
                    { false, false, false, false, false }
                },
                2,
                1,
                0
            )
        },
        {
            10,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true,  true, false },
                    { false, false,  true,  true,  true },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                4,
                2,
                0
            )
        },
        {
            11,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    { false,  true,  true, false, false },
                    { false, false,  true,  true, false },
                    { false, false, false, false, false }
                },
                8,
                6,
                3
            )
        },
        {
            12,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true, false, false, false },
                    { false,  true,  true, false, false },
                    { false, false,  true,  true, false },
                    { false, false, false, false, false }
                },
                10,
                4,
                3
            )
        },
        {
            13,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false,  true,  true,  true, false },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                5,
                4,
                2
            )
        },
        {
            14,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false,  true,  true, false, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                3,
                1,
                0
            )
        },
        {
            15,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    {  true,  true,  true,  true, false },
                    { false, false,  true,  true, false },
                    { false, false, false, false, false }
                },
                10,
                5,
                3
            )
        },
        {
            16,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    {  true,  true,  true, false, false },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                4,
                2,
                1
            )
        },
        {
            17,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    {  true, false, false, false, false },
                    {  true,  true,  true,  true, false },
                    { false, false, false,  true, false },
                    { false, false, false, false, false }
                },
                1,
                2,
                0
            )
        },
        {
            18,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    {  true,  true,  true,  true,  true },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                7,
                1,
                1
            )
        },
        {
            19,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    { false,  true,  true,  true,  true },
                    { false,  true, false, false, false },
                    { false, false, false, false, false }
                },
                10,
                3,
                2
            )
        },
        {
            20,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    {  true,  true,  true, false, false },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                4,
                6,
                2
            )
        },
        {
            21,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false, false,  true,  true,  true },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                5,
                5,
                2
            )
        },
        {
            22,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true,  true, false },
                    { false,  true,  true, false, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                3,
                2,
                1
            )
        },
        {
            23,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    {  true,  true,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                3,
                3,
                1
            )
        },
        {
            24,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    { false, false,  true,  true,  true },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                2,
                3,
                1
            )
        },
        {
            25,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false,  true,  true,  true, false },
                    { false,  true, false,  true, false },
                    { false, false, false, false, false }
                },
                3,
                6,
                2
            )
        },
        {
            26,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    { false, false,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                7,
                6,
                3
            )
        },
        {
            27,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    {  true, false, false,  true, false },
                    {  true,  true,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                1,
                5,
                1
            )
        },
        {
            28,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false,  true,  true, false, false },
                    { false,  true,  true, false, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                6,
                5,
                2
            )
        },
        {
            29,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    { false,  true,  true,  true, false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                2,
                2,
                0
            )
        },
        {
            30,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    {  true,  true,  true,  true, false },
                    { false,  true,  true, false, false },
                    { false, false, false, false, false }
                },
                7,
                4,
                2
            )
        },
        {
            31,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false,  true,  true,  true,  true },
                    { false, false,  true, false, false },
                    { false, false, false, false, false }
                },
                0,
                3,
                1
            )
        },
        {
            32,
            (
                new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false,  true, false, false },
                    { false,  true,  true,  true,  true },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                3,
                4,
                1
            )
        },
    };
}