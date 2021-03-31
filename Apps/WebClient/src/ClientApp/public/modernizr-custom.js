/*! modernizr 3.6.0 (Custom Build) | MIT *
 * https://modernizr.com/download/?-es6number-setclasses !*/
!(function (e, n, s) {
    function o(e, n) {
        return typeof e === n;
    }
    function a() {
        var e, n, s, a, i, u, f;
        for (var l in r)
            if (r.hasOwnProperty(l)) {
                if (
                    ((e = []),
                    (n = r[l]),
                    n.name &&
                        (e.push(n.name.toLowerCase()),
                        n.options &&
                            n.options.aliases &&
                            n.options.aliases.length))
                )
                    for (s = 0; s < n.options.aliases.length; s++)
                        e.push(n.options.aliases[s].toLowerCase());
                for (
                    a = o(n.fn, "function") ? n.fn() : n.fn, i = 0;
                    i < e.length;
                    i++
                )
                    (u = e[i]),
                        (f = u.split(".")),
                        1 === f.length
                            ? (Modernizr[f[0]] = a)
                            : (!Modernizr[f[0]] ||
                                  Modernizr[f[0]] instanceof Boolean ||
                                  (Modernizr[f[0]] = new Boolean(
                                      Modernizr[f[0]]
                                  )),
                              (Modernizr[f[0]][f[1]] = a)),
                        t.push((a ? "" : "no-") + f.join("-"));
            }
    }
    function i(e) {
        var n = f.className,
            s = Modernizr._config.classPrefix || "";
        if ((l && (n = n.baseVal), Modernizr._config.enableJSClass)) {
            var o = new RegExp("(^|\\s)" + s + "no-js(\\s|$)");
            n = n.replace(o, "$1" + s + "js$2");
        }
        Modernizr._config.enableClasses &&
            ((n += " " + s + e.join(" " + s)),
            l ? (f.className.baseVal = n) : (f.className = n));
    }
    var t = [],
        r = [],
        u = {
            _version: "3.6.0",
            _config: {
                classPrefix: "",
                enableClasses: !0,
                enableJSClass: !0,
                usePrefixes: !0,
            },
            _q: [],
            on: function (e, n) {
                var s = this;
                setTimeout(function () {
                    n(s[e]);
                }, 0);
            },
            addTest: function (e, n, s) {
                r.push({ name: e, fn: n, options: s });
            },
            addAsyncTest: function (e) {
                r.push({ name: null, fn: e });
            },
        },
        Modernizr = function () {};
    (Modernizr.prototype = u), (Modernizr = new Modernizr());
    var f = n.documentElement,
        l = "svg" === f.nodeName.toLowerCase();
    Modernizr.addTest(
        "es6number",
        !!(
            Number.isFinite &&
            Number.isInteger &&
            Number.isSafeInteger &&
            Number.isNaN &&
            Number.parseInt &&
            Number.parseFloat &&
            Number.isInteger(Number.MAX_SAFE_INTEGER) &&
            Number.isInteger(Number.MIN_SAFE_INTEGER) &&
            Number.isFinite(Number.EPSILON)
        )
    ),
        a(),
        i(t),
        delete u.addTest,
        delete u.addAsyncTest;
    for (var c = 0; c < Modernizr._q.length; c++) Modernizr._q[c]();
    e.Modernizr = Modernizr;
})(window, document);
