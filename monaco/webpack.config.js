import CssMinimizerPlugin from 'css-minimizer-webpack-plugin'
import HtmlWebPackPlugin from 'html-webpack-plugin'
import MiniCssExtractPlugin from 'mini-css-extract-plugin'

export default {
  resolve: {
    extensions: ['.mjs', '.js', '.ts']
  },
  module: {
    rules: [
      {
        test: /\.css$/,
        use: [MiniCssExtractPlugin.loader, 'css-loader']
      },
      {
        // Monaco editor uses .ttf icons.
        test: /\.(svg|ttf)$/,
        type: 'asset/resource'
      },
      {
        test: /\.ts$/,
        loader: 'ts-loader',
        options: { transpileOnly: true }
      }
    ]
  },
  optimization: {
    moduleIds: 'size',
    innerGraph: true,
    minimizer: ['...', new CssMinimizerPlugin()],
    chunkIds: 'total-size',
    runtimeChunk: 'single',
  },
  plugins: [
    new HtmlWebPackPlugin(),
    new MiniCssExtractPlugin({
      filename: '[contenthash].css',
      //filename: 'bundle.css',
    }),
  ],
  output: {
    clean: true,
    filename: '[contenthash].js',
    //filename: 'bundle.js',
    asyncChunks: false,
  },
  devtool: 'source-map',
  performance: {
    maxAssetSize: 10000000,
    maxEntrypointSize: 10000000,
  },
}
