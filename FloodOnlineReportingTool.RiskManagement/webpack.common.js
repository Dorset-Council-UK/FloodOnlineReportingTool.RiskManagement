const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

//module.exports = {
//    entry: {
//        Searchbox: './Components/Search/Searchbox.razor.scss',
//    },
//    experiments: {
//        outputModule: true,
//    },
//    output: {
//        path: path.resolve(__dirname, 'Components/Search'),
//        filename: '[name].razor.js', //Blank js is required
//    },
//    module: {
//        rules: [
//            {
//                test: /\.scss$/i,
//                use: [
//                    MiniCssExtractPlugin.loader, // Extracts CSS into files
//                    'css-loader',                // Translates CSS into CommonJS
//                    'sass-loader'                // Compiles Sass to CSS
//                ],
//            }
//        ]
//    },
//    plugins: [
//        // Extract the CSS into a separate file
//        new MiniCssExtractPlugin({
//            filename: '[name].razor.css'
//        })
//    ]
//};

const grid = {
    name: 'grid',
    entry: {
        GridCard: './Components/Layout/GridCard.razor.scss',
    },
    experiments: {
        outputModule: true,
    },
    output: {
        filename: '[name].razor.js', //Blank js is required
        path: path.resolve(__dirname, 'Components/Layout')
    },
    module: {
        rules: [
            {
                test: /\.scss$/i,
                use: [
                    MiniCssExtractPlugin.loader, // Extracts CSS into files
                    'css-loader',                // Translates CSS into CommonJS
                    'sass-loader'                // Compiles Sass to CSS
                ],
            }
        ]
    },
    plugins: [
        // Extract the CSS into a separate file
        new MiniCssExtractPlugin({
            filename: '[name].razor.css'
        })
    ]
};

const search = {
    name: 'search',
    entry: {
        Searchbox: './Components/Search/Searchbox.razor.scss',
    },
    experiments: {
        outputModule: true,
    },
    output: {
        filename: '[name].razor.js', //Blank js is required
        path: path.resolve(__dirname, 'Components/Search')
    },
    module: {
        rules: [
            {
                test: /\.scss$/i,
                use: [
                    MiniCssExtractPlugin.loader, // Extracts CSS into files
                    'css-loader',                // Translates CSS into CommonJS
                    'sass-loader'                // Compiles Sass to CSS
                ],
            }
        ]
    },
    plugins: [
        // Extract the CSS into a separate file
        new MiniCssExtractPlugin({
            filename: '[name].razor.css'
        })
    ]
};

MergedConfig = [grid, search]

module.exports = grid;