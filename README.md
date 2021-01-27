# Umbraco Tinifier 2.0

**Notice: This package addresses several issues preventing TinifierNext from running. Since the author/owner of that package are not reachable, this new package was created instead.**

## Overview
The official and free Umbraco [TinyPNG][tp] package for image compressing. Tinifier allows to dramatically reduce the size of PNG and JPEG images which positively influences  page loading time.

[TinyPNG][tp] uses smart lossy compression techniques to reduce the file size of your PNG files. By selectively decreasing the number of colors in the image, fewer bytes are required to store the data. The effect is nearly invisible, but it makes a huge difference in file size. 

[TinyPNG][tp] provides an API which allows compressing images programmatically. 500 successful requests per month are available for free usage. It can be not enough for large enterprise websites, so check prices on the TinyPNG website before the start.

## Quick start [(download .pdf with screens)][qs]
*(the pdf is outdated, but the principals still apply)*
1. Install Tinifier2 package
2. Register account in the [TinyPNG][tp] and get API key
3. Add the Tinifier section for your user in the Users section.
4. Go to the Tinifier settings and set API key.
5. Tinify (compress) an appropriate image 
6. Your visitors are happy with fast loading pages!


## Features
- Optimize individual images from the Media
- Folder optimization
- Supported image formats: PNG and JPEG
- Optimized image stats 
- API requests widget
- Total saved bytes widget
- Image optimization on upload
- Umbraco 8 support
- Azure blob storage support
- Top 50 tinified images
- Save metadata
- Optimize ImageCropper images on upload
- Tinify everything (all media and crops)
- Organize images by dates in Media

## Nuget package
https://www.nuget.org/packages/Our.Umbraco.Tinifier

## License

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

[bd]: http://backend-devs.com/
[tp]: https://tinypng.com
[qs]: https://our.umbraco.org/FileDownload?id=17908
