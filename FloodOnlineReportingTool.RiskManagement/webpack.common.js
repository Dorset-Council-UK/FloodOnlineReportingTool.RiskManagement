const path = require('path');
const { globSync } = require('glob');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

// Dynamically load all *.razor.ts files under the Components folder
const componentEntries = Object.fromEntries(
    globSync('./Components/**/*.razor.ts', { withFileTypes: true })
        .map(o => [
            `components/${o.name.replace(/\.razor\.ts$/, '').toLowerCase()}`,
            `./${o.relativePosix()}`
        ]
        )
);

module.exports = {
    entry: {
        app: './Scripts/app.ts',
        ...componentEntries
    },
    resolve: {
        extensions: ['.ts', '.js']
    },
    experiments: {
        outputModule: true,
    },
    output: {
        filename: 'js/[name].js',
        path: path.resolve(__dirname, 'wwwroot'),
        library: {
            type: 'module'
        }
    },
    module: {
        rules: [
            {
                test: /\.([cm]?ts|tsx)$/,
                loader: 'ts-loader',
                exclude: /node_modules/,
            },
            {
                test: /\.s[ac]ss$/i,
                use: [
                    MiniCssExtractPlugin.loader, // Extracts CSS into separate files
                    {
                        loader: 'css-loader', // Creates `style` nodes from JS strings
                        options: {
                            url: false // This tells css-loader not to resolve url() paths
                        }
                    },
                    {
                        loader: 'sass-loader', // Compiles Sass to CSS
                        options: {
                            sassOptions: {
                                quietDeps: true,
                                includePaths: ['node_modules']
                            }
                        }
                    }
                ]
            },
            {
                test: /\.css$/i, // Extracts imported CSS in TypeScript into separate files
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader', // Creates `style` nodes from JS strings
                        options: {
                            url: false // This tells css-loader not to resolve url() paths
                        }
                    },
                ]
            }
        ]
    },
    plugins: [
        // Extract the CSS into a separate file
        new MiniCssExtractPlugin({
            filename: 'css/[name].css'
        })
    ]
}