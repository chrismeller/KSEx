So the main problem here is that we have a lot of outliers - people who literally came back 12 hours later and viewed another page.

We could easily establish some "expected" values and discard anything outside of that range, but without knowing more about the structure and content of the pages I can't really make any reasonable guesses as to what those would be.

There's also a PHP script I threw together to check my math, since I wasn't entirely sure I wasn't being too clever for my own good with the nested LINQ queries. (The .02 difference on Catalog pages is a rounding difference between PHP and .Net).
