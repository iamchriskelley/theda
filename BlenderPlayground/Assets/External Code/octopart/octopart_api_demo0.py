import operator
import json
import urllib

'''
MPN
    specs, etc
    offers
        in_stock_quantity
        breaks
            MOQ
            price
'''
class PartResult:
    def __init__(self, mpn):
        self.mpn = mpn
        self.offers = []
        self.best_offers = {}
    def add_offer(self, offer):
        self.offers.append(offer)
    def print_offers(self):
        for offer in self.offers:
            offer.print_offer()
    def find_best_offers(self):
        print "crunching best offers for " + self.mpn + "....."

class Offer:
    def __init__(self, _seller, _isq, _moq, _breaks):
        self.seller = _seller
        self.isq = _isq
        self.moq = _moq
        self.breaks = _breaks

    def print_offer(self):
        print self.seller, str(self.isq), str(self.moq), str(self.breaks)
        

def get_octopart_results(partname, private_key, quantity, auth_only=False):
    print_urls=False
    url = 'http://octopart.com/api/v3/parts/match?'
    url += '&queries=[{"mpn":"' + partname + '*", "limit":20}]'
    url += '&apikey=' + private_key
    url += '&include=specs'
    data = urllib.urlopen(url).read()
    
    results = {}
    response = json.loads(data)
    D = {}
    default_price = 10000000.0
    minquant = 1
    for result in response['results']:
        for item in result['items']:
            offer_count = len(item['offers'])
            if offer_count > 0:
                mpn = item['mpn']
                D[mpn] = {}
                P = PartResult(mpn)
                out = item['mpn'] + " [" + str(offer_count) + "]"
                if 'case_package' in item['specs']:
                    s = item['specs']['case_package']['display_value']
                    if 'pin_count' in item['specs']:
                        s += "-" + item['specs']['pin_count']['display_value']
                    out += ". " + s
                for offer in item['offers']:
                    if "USD" in offer["prices"]:
                        seller_name = offer["seller"]["name"]
                        seller_quant = offer["in_stock_quantity"]
                        seller_moq = offer["moq"]
                        seller_prices = list(offer["prices"]["USD"])
                        O = Offer(seller_name, seller_quant, seller_moq, seller_prices)
                        P.add_offer(O)
                        passes = True
                        if auth_only and not offer["is_authorized"]: passes = False
                        if passes:
                            for ofr in offer["prices"]["USD"]:
                                #print ofr[0]
                                if ofr[0] not in D[mpn]:
                                    D[mpn][ofr[0]] = [(seller_name, float(ofr[1]), seller_quant)]
                                else:
                                    D[mpn][ofr[0]].append((seller_name, float(ofr[1]), seller_quant))
                results[mpn] = P
                P.find_best_offers()
                #P.print_offers()
                
    #print D
    if quantity >= 1:
        for k,v in D.iteritems():
            cheapest_price = default_price
            min_buy_quant = 0
            buy_from = "foobar"
            #print k
            breaks = sorted(list(v.keys()))
            idx = 0
            for i in range(len(breaks)):
                v[breaks[idx]].sort(key=operator.itemgetter(1))
                best_per = v[breaks[i]][0]
                if best_per[1] < cheapest_price:
                    min_buy_quant = breaks[i]
                    buy_from = best_per[0]
                    cheapest_price = best_per[1]
                if quantity >= breaks[i]:
                    #print breaks[i], best_per
                    idx = i
                else:
                    break
            #print "Best price: buy at least " + str(min_buy_quant) + " from " + buy_from + " for $" + str(cheapest_price)

    
    
get_octopart_results("ATMEGA328", "f5c651a5", 150, True)

