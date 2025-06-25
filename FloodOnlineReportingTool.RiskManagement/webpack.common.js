const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
    entry: {
        GridCard: './Components/Layout/GridCard.razor.scss',
        Searchbox: './Components/Search/Searchbox.razor.scss',
    },
    experiments: {
        outputModule: true,
    },
    output: {
        filename: '[name].razor.css', //Not used in this scenario
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
}